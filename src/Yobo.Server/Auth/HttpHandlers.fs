module Yobo.Server.Auth.HttpHandlers

open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Giraffe
open Yobo.Shared.Auth.Communication
open Yobo.Shared.Auth.Validation
open FSharp.Control.Tasks
open Yobo.Shared.Communication
open FSharp.Rop

let private login (l:Request.Login) =
    task {
        return
            result {
                let! _ = l |> ServerResult.ofValidation validateLogin
                return "OK"
            }
    }

let private authService : AuthService = {
    GetToken = login >> Async.AwaitTask
}

let authServiceHandler : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder AuthService.RouteBuilder
    |> Remoting.fromValue authService
    |> Remoting.buildHttpHandler