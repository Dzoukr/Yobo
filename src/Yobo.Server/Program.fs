module Yobo.Server.Program

open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open System.Threading.Tasks
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Giraffe

let authService : Yobo.Shared.Auth.Communication.AuthService = {
    Login = fun _ -> async { return Ok "" }
}

let authServiceHandler : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Auth.Communication.AuthService.RouteBuilder
    |> Remoting.fromValue authService
    |> Remoting.buildHttpHandler

let webApp = choose [ authServiceHandler ]

[<FunctionName("Index")>]
let run ([<HttpTrigger (AuthorizationLevel.Anonymous, Route = "{*any}")>] req : HttpRequest, context : ExecutionContext, log : ILogger) =
    let hostingEnvironment = req.HttpContext.GetHostingEnvironment()
    hostingEnvironment.ContentRootPath <- context.FunctionAppDirectory
    webApp (Some >> Task.FromResult) req.HttpContext