module Yobo.Server.Startup

open System
open System.IO
open System.Reflection
open Giraffe
open Microsoft.Azure.Functions.Extensions.DependencyInjection
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host.Config
open Microsoft.Azure.WebJobs.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Yobo.Libraries.Authentication
open FSharp.Control.Tasks
open FSharp.Rop.TaskResult
open Microsoft.Data.SqlClient
open Yobo.Server.Auth
open Yobo.Shared.Domain

module private Configuration =
   
    let private getFunctionsRootPath () =
        Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot")
        |> Option.ofObj
        |> Option.orElseWith (fun _ ->
            Environment.GetEnvironmentVariable("HOME")
            |> Option.ofObj
            |> Option.map (sprintf "%s/site/wwwroot")
        )
        |> Option.defaultWith (fun _ -> Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    
    let load () =
        let rootPath = getFunctionsRootPath ()
        (ConfigurationBuilder())
            .SetBasePath(rootPath)
            .AddJsonFile("local.settings.json", true)
            .AddEnvironmentVariables().Build()

let private withSqlConnection builder fn =
    let conn = builder()
    fn conn

module AuthRoot =
    
    let compose sqlConnectionBuilder sendEmail emailBuilder (cfg:IConfigurationRoot) =
        
        let sql fn = withSqlConnection sqlConnectionBuilder fn
        
        // config
        let issuer = cfg.["AuthIssuer"]
        let audience = cfg.["AuthAudience"]
        let secret = cfg.["AuthSecret"]
        let tokenLifetime = cfg.["AuthTokenLifetime"] |> TimeSpan.Parse
        let pars = Jwt.getParameters audience issuer secret
        
        let queries = {
            TryGetUserByEmail = sql Auth.Database.Queries.tryGetUserByEmail
            TryGetUserById = sql Auth.Database.Queries.tryGetUserById
        }
        
        let handleEvents conn evns = task {
            for e in evns do
                do! Auth.DbEventHandler.handle conn e
                do! Auth.EmailEventHandler.handle sendEmail emailBuilder queries.TryGetUserById e
        }
        
        let toExn = Result.mapError ServerError.Authentication >> ServerError.ofResult
        
        {
            CreateToken = Jwt.createJwtToken audience issuer secret tokenLifetime >> fun x -> x.Token
            ValidateToken = Jwt.validateToken pars >> Option.map List.ofSeq
            VerifyPassword = Password.verifyPassword
            CreatePasswordHash = Password.createHash
            Queries = queries
            CommandHandler = {
                Register = fun args -> task {
                    let! projections = sql Auth.Database.Projections.getAll
                    return! args |> CommandHandler.register projections |> toExn |> sql handleEvents
                }
                ActivateAccount = fun args -> task {
                    let! projections = sql Auth.Database.Projections.getAll
                    return! args |> CommandHandler.activate projections |> toExn |> sql handleEvents
                }
                ForgottenPassword = fun args -> task {
                    let! projections = sql Auth.Database.Projections.getAll
                    return! args |> CommandHandler.initiatePasswordReset projections |> toExn |> sql handleEvents
                }
                ResetPassword = fun args -> task {
                    let! projections = sql Auth.Database.Projections.getAll
                    return!
                        args
                        |> CommandHandler.completePasswordReset projections
                        |> toExn
                        |> sql handleEvents
                }
            }
        } : AuthRoot

module UserAccountRoot =
    let compose sqlConnectionBuilder (cfg:IConfigurationRoot) =
        let sql fn = withSqlConnection sqlConnectionBuilder fn
        
        {
            Queries = {
                TryGetUserInfo = sql UserAccount.Database.Queries.tryGetUserById
            }    
        }



module CompositionRoot =
    open Yobo.Libraries.Emails
    
    type PartialEmail = {| To:Address; Subject:string; Message:string |}
    
    let compose (cfg:IConfigurationRoot) =
        Dapper.FSharp.OptionTypes.register()
        
        let sendEmail partial =
            let from = { Name = cfg.["EmailsFromName"]; Email = cfg.["EmailsFromEmail"] }
            let send = Yobo.Libraries.Emails.MailjetSender.sendEmail cfg.["MailjetApiKey"] cfg.["MailjetSecretKey"] >> fun _ -> task { return () }
            partial
            |> (fun (x:PartialEmail) -> { From = from; To = [x.To]; Bcc = []; Cc = []; Subject = x.Subject; PlainTextMessage = ""; HtmlMessage = x.Message })
            |> send
        
        let emailBuilder = EmailTemplates.getDefault (Uri cfg.["ServerBaseUrl"])
        let sqlConnectionBuilder () = new SqlConnection(cfg.["ReadDbConnectionString"])
        
        {
            Auth = AuthRoot.compose sqlConnectionBuilder sendEmail emailBuilder cfg
            UserAccount = UserAccountRoot.compose sqlConnectionBuilder cfg
        } : CompositionRoot


type private InjectCompositionRoot(root) =
    
    interface IExtensionConfigProvider with
        member this.Initialize (context: ExtensionConfigContext) =
            context
                .AddBindingRule<Attributes.CompositionRootAttribute>()
                .BindToInput<CompositionRoot>(fun x -> root)
            |> ignore

        
type WebJobsStartup() =
    interface IWebJobsStartup with
        member this.Configure(builder:IWebJobsBuilder) =
            let root = Configuration.load () |> CompositionRoot.compose

            root
            |> builder.Services.AddSingleton<CompositionRoot>
            |> ignore
            builder.Services.AddGiraffe() |> ignore
            builder.AddExtension(InjectCompositionRoot(root)) |> ignore
    
[<assembly: WebJobsStartup(typeof<WebJobsStartup>)>] do ()        
