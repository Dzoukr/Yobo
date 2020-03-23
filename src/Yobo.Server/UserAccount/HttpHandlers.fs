module Yobo.Server.UserAccount.HttpHandlers

open System
open System.Security.Claims
open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Giraffe
open FSharp.Control.Tasks
open Yobo.Server.Auth.Domain
open Yobo.Shared.Auth.Validation
open FSharp.Rop.Result
open FSharp.Rop.TaskResult
open Microsoft.AspNetCore.Http
open Yobo.Server.Auth
open Domain
open Microsoft.AspNetCore.Http
open Yobo.Server
open Yobo.Shared.Domain
open Yobo.Shared.UserAccount.Communication
open FSharp.Control.Tasks

let private userAccountService (root:CompositionRoot) userId  : UserAccountService =
    {
        GetUserInfo =
            (fun _ -> task { return ({ Id = userId; FirstName = "AAA"; LastName = "BBB" } : Yobo.Shared.UserAccount.Communication.Response.UserInfo) }) 
            >> Async.AwaitTask
    }

let private withUser (proxyBuilder:Guid -> 'proxy) (ctx:HttpContext) =
    let userId =
        ctx.User.Claims
        |> Seq.find (fun x -> x.Type = "Id")
        |> fun x -> x.Value
        |> Guid
    userId |> proxyBuilder

let userAccountServiceHandler (root:CompositionRoot) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder UserAccountService.RouteBuilder
    |> Remoting.fromContext (withUser (userAccountService root))
    |> Remoting.withErrorHandler Remoting.errorHandler
    |> Remoting.buildHttpHandler