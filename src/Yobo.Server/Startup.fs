module Yobo.Server.Startup

open Giraffe
open Microsoft.Azure.Functions.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Configuration

type Startup() =
    inherit FunctionsStartup()
    override this.Configure (builder:IFunctionsHostBuilder) =
        let cfg = (ConfigurationBuilder())
                      .AddJsonFile("local.settings.json", true)
                      .AddEnvironmentVariables().Build()
                  |> Configuration.load
        
        builder.Services.AddGiraffe() |> ignore
        builder.Services.AddSingleton<Configuration>(cfg) |> ignore
    
[<assembly: FunctionsStartup(typeof<Startup>)>]
do ()