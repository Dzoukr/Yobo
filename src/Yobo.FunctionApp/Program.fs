module Yobo.FunctionApp.Program

open Microsoft.Azure.WebJobs
open Microsoft.Extensions.Logging
open Giraffe
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open System.Threading.Tasks
open System
open Microsoft.Extensions.Configuration
open Yobo.FunctionApp.Configuration

[<FunctionName("Index")>]
let run ([<HttpTrigger (AuthorizationLevel.Anonymous, Route = "{*any}")>] req : HttpRequest, context : ExecutionContext, log : ILogger) =
    let cfg = (ConfigurationBuilder()).AddJsonFile("local.settings.json", true).AddEnvironmentVariables().Build() |> Configuration.load
    let hostingEnvironment = req.HttpContext.GetHostingEnvironment()
    hostingEnvironment.ContentRootPath <- context.FunctionAppDirectory
    let func = Some >> Task.FromResult
    task {
        let! _ = (WebApp.webApp cfg) func req.HttpContext
        req.HttpContext.Response.Body.Flush() //workaround https://github.com/giraffe-fsharp/Giraffe.AzureFunctions/issues/1
    } :> Task    