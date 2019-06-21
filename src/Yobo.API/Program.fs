module Yobo.API.Program

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.Extensions.Configuration

#if DEBUG
let publicPath = Path.GetFullPath "../Yobo.Client/public"
#else
let publicPath = Path.GetFullPath "wwwroot"
#endif

let configureApp cfg (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    app.UseDefaultFiles()
       .UseStaticFiles()
       .UseGiraffe (WebApp.webApp cfg)

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

let cfg = 
    (ConfigurationBuilder())
        .AddJsonFile(IO.Directory.GetCurrentDirectory() + "\config.development.json", true)
        .AddEnvironmentVariables()
        .Build()
    |> Configuration.load

WebHostBuilder()
    .UseKestrel()
    .UseIISIntegration()
    .UseWebRoot(publicPath)
    .UseContentRoot(publicPath)
    .Configure(Action<IApplicationBuilder> (configureApp cfg))
    .ConfigureServices(configureServices)
    .UseUrls("http://0.0.0.0:8085/")
    .Build()
    .Run()