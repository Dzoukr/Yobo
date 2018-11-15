open System
open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

open FSharp.Control.Tasks.V2
open Giraffe
open Yobo.Shared

open Giraffe.Serialization

#if DEBUG
let publicPath = Path.GetFullPath "../Yobo.Client/public"
#else
let publicPath = Path.GetFullPath "wwwroot"
#endif

let port = 8085us

let getInitCounter () : Task<Counter> = task { return { Value = 999; Message = "Nazdar volecku"} }

open Yobo.Shared.Communication
open Yobo.Shared.Text
open Yobo.Shared.Validation


let webApp =
    
    route "/api/register" >=>
        fun next ctx ->
            task {
                let err = TextValue.FirstName |> ValidationError.IsEmpty |> ServerError.ValidationError
                return! RequestErrors.BAD_REQUEST err next ctx
            }

let configureApp (app : IApplicationBuilder) =
    app.UseDefaultFiles()
       .UseStaticFiles()
       .UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore
    services.AddSingleton<IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer()) |> ignore

WebHostBuilder()
    .UseKestrel()
    .UseIISIntegration()
    .UseWebRoot(publicPath)
    .UseContentRoot(publicPath)
    .Configure(Action<IApplicationBuilder> configureApp)
    .ConfigureServices(configureServices)
    .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
    .Build()
    .Run()