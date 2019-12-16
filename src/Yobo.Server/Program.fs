module Yobo.Server.Program

open System.Threading.Tasks
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Giraffe
open Configuration

let webApp (cfg:Configuration) = choose [
    Auth.HttpHandlers.authServiceHandler cfg
]


[<FunctionName("Index")>]
let run ([<HttpTrigger (AuthorizationLevel.Anonymous, Route = "{*any}")>] req : HttpRequest, context : ExecutionContext, log : ILogger) =
    let hostingEnvironment = req.HttpContext.GetHostingEnvironment()
    hostingEnvironment.ContentRootPath <- context.FunctionAppDirectory
    webApp (req.HttpContext.GetService<Configuration>()) (Some >> Task.FromResult) req.HttpContext