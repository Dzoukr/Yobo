module Yobo.Server.Users.HttpHandlers

open Giraffe
open Yobo.Server
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Yobo.Libraries.Tasks
open Yobo.Shared.Users.Communication

let private usersService (root:CompositionRoot) userId : UsersService =
    {
        GetAllUsers = root.Users.Queries.GetAllUsers >> Async.AwaitTask
    }

let usersServiceHandler (root:CompositionRoot) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder UsersService.RouteBuilder
    |> Remoting.fromContext (Auth.HttpHandlers.withUser (usersService root))
    |> Remoting.withErrorHandler Remoting.errorHandler
    |> Remoting.buildHttpHandler