module Yobo.API.Program

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

#if DEBUG
let publicPath = Path.GetFullPath "../Yobo.Client/public"
#else
let publicPath = Path.GetFullPath "wwwroot"
#endif

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    app.UseDefaultFiles()
       .UseStaticFiles()
       .UseGiraffe (Routes.webApp env.WebRootPath)

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

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