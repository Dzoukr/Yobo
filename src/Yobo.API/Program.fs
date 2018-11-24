module Yobo.API.Program

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection

open Giraffe
open Giraffe.Serialization


#if DEBUG
let publicPath = Path.GetFullPath "../Yobo.Client/public"
#else
let publicPath = Path.GetFullPath "wwwroot"
#endif

let configureApp (app : IApplicationBuilder) =
    app.UseDefaultFiles()
       .UseStaticFiles()
       .UseGiraffe Routes.webApp

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
    .UseUrls("http://0.0.0.0:8085/")
    .Build()
    .Run()