module Yobo.Server.Startup

open Giraffe
open Microsoft.Azure.Functions.Extensions.DependencyInjection

type Startup() =
    inherit FunctionsStartup()
    override this.Configure (builder:IFunctionsHostBuilder) =
        builder.Services.AddGiraffe() |> ignore
    
[<assembly: FunctionsStartup(typeof<Startup>)>]
do ()