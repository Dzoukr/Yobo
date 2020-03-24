module Yobo.Server.UserAccount.HttpHandlers

open System
open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Giraffe
open Microsoft.AspNetCore.Http
open Yobo.Libraries.Tasks
open Yobo.Server
open Yobo.Shared.Domain
open Yobo.Shared.UserAccount.Communication

let private getUserInfo (root:CompositionRoot) userId () =
    userId
    |> root.UserAccount.Queries.TryGetUserInfo
    |> Task.map (ServerError.ofOption <| DatabaseItemNotFound(userId))

let private userAccountService (root:CompositionRoot) userId : UserAccountService =
    {
        GetUserInfo = getUserInfo root userId >> Async.AwaitTask
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