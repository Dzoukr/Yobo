module Yobo.Server.Program

open System.Threading.Tasks
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Giraffe
open Yobo.Server.CompositionRoot

let webApp (root:CompositionRoot) =
    choose [
        Auth.HttpHandlers.authServiceHandler root
    ]

[<FunctionName("Index")>]
let run ([<HttpTrigger (AuthorizationLevel.Anonymous, Route = "{*any}")>] req : HttpRequest, context : ExecutionContext, log : ILogger) =
    let hostingEnvironment = req.HttpContext.GetHostingEnvironment()
    hostingEnvironment.ContentRootPath <- context.FunctionAppDirectory
    webApp (req.HttpContext.GetService<CompositionRoot>()) (Some >> Task.FromResult) req.HttpContext