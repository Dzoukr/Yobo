module Yobo.Server.Core.UserAccount.HttpHandlers

open System
open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Giraffe
open Microsoft.AspNetCore.Http
open Yobo.Libraries.Tasks
open Yobo.Server
open Yobo.Shared.Errors
open Yobo.Shared.Core.UserAccount.Communication

let private getUserInfo (root:CompositionRoot) userId () =
    userId
    |> root.UserAccount.Queries.GetUserInfo

let private userAccountService (root:CompositionRoot) userId : UserAccountService =
    {
        GetUserInfo = getUserInfo root userId >> Async.AwaitTask
        GetMyLessons = (fun _ -> root.UserAccount.Queries.GetUserLessons userId) >> Async.AwaitTask
    }

let userAccountServiceHandler (root:CompositionRoot) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder UserAccountService.RouteBuilder
    |> Remoting.fromContext (Auth.HttpHandlers.withUser (userAccountService root))
    |> Remoting.withErrorHandler Remoting.errorHandler
    |> Remoting.buildHttpHandler