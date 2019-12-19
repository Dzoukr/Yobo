module Yobo.Server.Startup

open Giraffe
open Microsoft.Azure.Functions.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Yobo.Server.CompositionRoot

type Startup() =
    inherit FunctionsStartup()
    override this.Configure (builder:IFunctionsHostBuilder) =
        let root = (ConfigurationBuilder())
                      .AddJsonFile("local.settings.json", true)
                      .AddEnvironmentVariables().Build()
                  |> CompositionRoot.compose
        
        builder.Services.AddGiraffe() |> ignore
        builder.Services.AddSingleton<CompositionRoot>(root) |> ignore
    
[<assembly: FunctionsStartup(typeof<Startup>)>]
do ()