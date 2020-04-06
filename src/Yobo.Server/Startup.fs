module Yobo.Server.Startup

open System
open System.IO
open System.Reflection
open System.Threading.Tasks
open Giraffe
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host.Config
open Microsoft.Azure.WebJobs.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Yobo.Libraries.Authentication
open FSharp.Control.Tasks
open Microsoft.Data.SqlClient
open Yobo.Server.Auth
open Yobo.Shared.Errors

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

module CompositionRoot =
    open Yobo.Libraries.Emails
    open Yobo.Libraries.Tasks
        
    type PartialEmail = {| To:Address; Subject:string; Message:string |}
    
    let compose (cfg:IConfigurationRoot) =
        Dapper.FSharp.OptionTypes.register()
        
        let sql fn =
            let conn = new SqlConnection(cfg.["ReadDbConnectionString"])
            fn conn
        
        // JWT config
        let issuer = cfg.["AuthIssuer"]
        let audience = cfg.["AuthAudience"]
        let secret = cfg.["AuthSecret"]
        let tokenLifetime = cfg.["AuthTokenLifetime"] |> TimeSpan.Parse
        let pars = Jwt.getParameters audience issuer secret
        
        let createPwdHash = Password.createHash
        
        // emailing
        let sendEmail partial =
            let from = { Name = cfg.["EmailsFromName"]; Email = cfg.["EmailsFromEmail"] }
            let send = Yobo.Libraries.Emails.MailjetSender.sendEmail cfg.["MailjetApiKey"] cfg.["MailjetSecretKey"] >> Task.ignore
            partial
            |> (fun (x:PartialEmail) -> { From = from; To = [x.To]; Bcc = []; Cc = []; Subject = x.Subject; PlainTextMessage = ""; HtmlMessage = x.Message })
            |> send
        let emailBuilder = EmailTemplates.getDefault (Uri cfg.["ServerBaseUrl"])
        
        // admin user
        let adminUser =
            {
                Id = System.Guid("f65203d4-60dd-4580-a31c-e538807ef720")
                Email = cfg.["AdminEmail"]
                FirstName = "Admin"
                LastName = "Admin"
                IsAdmin = true
                IsActivated = true
                Credits = 0
                CreditsExpiration = None
                //CashReservationBlockedUntil = None
            } : Yobo.Shared.Core.UserAccount.Domain.Queries.UserAccount
        
        let adminPwd = cfg.["AdminPassword"] |> createPwdHash       
        
        {
            Auth =
                let tryGetUserByEmail (email:string) =
                    if email = adminUser.Email then
                        ({
                            Id = adminUser.Id
                            Email = adminUser.Email
                            PasswordHash = adminPwd
                            FirstName = adminUser.FirstName
                            LastName = adminUser.LastName
                            IsAdmin = true
                        } : Auth.Domain.Queries.AuthUserView)
                        |> Some
                        |> Task.FromResult
                    else
                        (sql Auth.Database.Queries.tryGetUserByEmail email)
                
                let queries = {
                    TryGetUserByEmail = tryGetUserByEmail
                    TryGetUserById = sql Auth.Database.Queries.tryGetUserById
                }

                let toExn = Result.mapError ServerError.Authentication >> ServerError.ofResult
                
                let handleEvents conn evns = task {
                    for e in evns do
                        do! Auth.DbEventHandler.handle conn e
                        do! Auth.EmailEventHandler.handle sendEmail emailBuilder queries.TryGetUserById e
                }
                
                {
                    CreateToken = Jwt.createJwtToken audience issuer secret tokenLifetime >> fun x -> x.Token
                    ValidateToken = Jwt.validateToken pars >> Option.map List.ofSeq
                    VerifyPassword = Password.verifyPassword
                    CreatePasswordHash = createPwdHash
                    Queries = queries
                    CommandHandler = {
                        Register = fun args -> task {
                            let! projections = sql Auth.Database.Projections.getAll
                            return! args |> Auth.CommandHandler.register projections |> toExn |> sql handleEvents
                        }
                        ActivateAccount = fun args -> task {
                            let! projections = sql Auth.Database.Projections.getAll
                            return! args |> Auth.CommandHandler.activate projections |> toExn |> sql handleEvents
                        }
                        ForgottenPassword = fun args -> task {
                            let! projections = sql Auth.Database.Projections.getAll
                            return! args |> Auth.CommandHandler.initiatePasswordReset projections |> toExn |> sql handleEvents
                        }
                        ResetPassword = fun args -> task {
                            let! projections = sql Auth.Database.Projections.getAll
                            return!
                                args
                                |> Auth.CommandHandler.completePasswordReset projections
                                |> toExn
                                |> sql handleEvents
                        }
                    }
                }
            UserAccount = {
                Queries =
                    {
                        TryGetUserInfo = (fun i ->
                            if i = adminUser.Id then adminUser |> Some |> Task.FromResult
                            else sql Core.UserAccount.Database.Queries.tryGetUserById i
                        )
                        GetUserLessons = sql Core.UserAccount.Database.Queries.getLessonsForUserId
                        GetUserOnlineLessons = sql Core.UserAccount.Database.Queries.getOnlineLessonsForUserId
                    }    
            }
            Admin =
                
                let toExn = Result.mapError ServerError.Domain >> ServerError.ofResult
                
                let handleEvents conn evns = task {
                    for e in evns do
                        do! Core.DbEventHandler.handle conn e
                }
                
                {
                    Queries = {
                        GetAllUsers = sql Core.Admin.Database.Queries.getAllUsers
                        GetLessons = sql Core.Admin.Database.Queries.getLessons
                        GetWorkshops = sql Core.Admin.Database.Queries.getWorkshops
                        GetOnlineLessons = sql Core.Admin.Database.Queries.getOnlineLessons
                    }
                    CommandHandler = {
                        AddCredits = fun args -> task {
                            let! projections = sql Core.Database.Projections.getById args.UserId
                            return! args |> Core.CommandHandler.addCredits projections |> toExn |> sql handleEvents
                        }
                        SetExpiration = fun args -> task {
                            let! projections = sql Core.Database.Projections.getById args.UserId
                            return! args |> Core.CommandHandler.setExpiration projections |> toExn |> sql handleEvents
                        }
                        CreateLesson = Core.CommandHandler.createLesson >> toExn >> sql handleEvents
                        CreateWorkshop = Core.CommandHandler.createWorkshop >> toExn >> sql handleEvents
                        CreateOnlineLesson = Core.CommandHandler.createOnlineLesson >> toExn >> sql handleEvents
                        ChangeLessonDescription = fun args -> task {
                            let! projections = sql Core.Database.Projections.getLessonById args.Id
                            return! args |> Core.CommandHandler.changeLessonDescription projections |> toExn |> sql handleEvents
                        }
                        CancelLesson = fun args -> task {
                            let! projections = sql Core.Database.Projections.getLessonById args.Id
                            return! args |> Core.CommandHandler.cancelLesson projections |> toExn |> sql handleEvents
                        }
                        DeleteLesson = fun args -> task {
                            let! projections = sql Core.Database.Projections.getLessonById args.Id
                            return! args |> Core.CommandHandler.deleteLesson projections |> toExn |> sql handleEvents
                        }
                        DeleteWorkshop = fun args -> task {
                            let! projections = sql Core.Database.Projections.getWorkshopById args.Id
                            return! args |> Core.CommandHandler.deleteWorkshop projections |> toExn |> sql handleEvents
                        }
                        ChangeOnlineLessonDescription = fun args -> task {
                            let! projections = sql Core.Database.Projections.getOnlineLessonById args.Id
                            return! args |> Core.CommandHandler.changeOnlineLessonDescription projections |> toExn |> sql handleEvents
                        }
                        CancelOnlineLesson = fun args -> task {
                            let! projections = sql Core.Database.Projections.getOnlineLessonById args.Id
                            return! args |> Core.CommandHandler.cancelOnlineLesson projections |> toExn |> sql handleEvents
                        }
                        DeleteOnlineLesson = fun args -> task {
                            let! projections = sql Core.Database.Projections.getOnlineLessonById args.Id
                            return! args |> Core.CommandHandler.deleteOnlineLesson projections |> toExn |> sql handleEvents
                        }
                    }
            }
            Reservations = {
                Queries = {
                    GetLessons = sql Core.Reservations.Database.Queries.getLessons
                    GetOnlineLessons = sql Core.Reservations.Database.Queries.getOnlineLessons
                }
            }                
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