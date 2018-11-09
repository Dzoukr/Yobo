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
let webApp =
    route "/api/init" >=>
        fun next ctx ->
            task {
                let! counter = getInitCounter()
                return! Successful.OK counter next ctx
            }

let configureApp (app : IApplicationBuilder) =
    app.UseDefaultFiles()
       .UseStaticFiles()
       .UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore
    let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
    fableJsonSettings.Converters.Add(Fable.JsonConverter())
    fableJsonSettings.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings) |> ignore

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