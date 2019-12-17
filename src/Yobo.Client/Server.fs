module Yobo.Client.Server

open Fable.Core
open Fable.Remoting.Client
open Yobo.Shared.Auth.Communication
open Yobo.Shared.Domain

module Cmd =
    open Elmish
    
    module OfAsync =
        let eitherResult f args resultMsg =
            let onError (ex:exn) = ex.Message |> ServerError.Exception |> Error |> resultMsg 
            Cmd.OfAsync.either f args resultMsg onError


[<Emit("config.baseUrl")>]
let baseUrl : string = jsNative

let authService =
    Remoting.createApi()
    |> Remoting.withRouteBuilder AuthService.RouteBuilder
    |> Remoting.withBaseUrl baseUrl
    |> Remoting.buildProxy<AuthService>