module Yobo.Client.Server

open Fable.Core
open Fable.Remoting.Client
open Fable.SimpleJson
open Yobo.Shared.Domain
open Yobo.Shared.Auth.Communication
open Yobo.Shared.UserAccount.Communication
open Yobo.Shared.Users.Communication

let exnToError (e:exn) : ServerError =
    match e with
    | :? Fable.Remoting.Client.ProxyRequestException as ex -> 
        if ex.StatusCode = 401 then
            AuthenticationError.InvalidOrExpiredToken |> ServerError.Authentication
        else
            try
                let serverError = Json.parseAs<{| error: ServerError |}>(ex.Response.ResponseBody)
                serverError.error
            with _ -> (ServerError.Exception(e.Message)) 
    | _ -> (ServerError.Exception(e.Message))

module Cmd =
    open Elmish
    
    module OfAsync =
        let eitherAsResult f args resultMsg =
            Cmd.OfAsync.either f args (Ok >> resultMsg) (exnToError >> Error >> resultMsg)


[<Emit("config.baseUrl")>]
let baseUrl : string = jsNative

let authService =
    Remoting.createApi()
    |> Remoting.withRouteBuilder AuthService.RouteBuilder
    |> Remoting.withBaseUrl baseUrl
    |> Remoting.buildProxy<AuthService>

let onUserAccountService (fn:UserAccountService -> 'a) =
    let token = TokenStorage.tryGetToken() |> Option.defaultValue ""
    Remoting.createApi()
    |> Remoting.withRouteBuilder UserAccountService.RouteBuilder
    |> Remoting.withBaseUrl baseUrl
    |> Remoting.withAuthorizationHeader (sprintf "Bearer %s" token)
    |> Remoting.buildProxy<UserAccountService>
    |> fn

let onUsersService (fn:UsersService -> 'a) =
    let token = TokenStorage.tryGetToken() |> Option.defaultValue ""
    Remoting.createApi()
    |> Remoting.withRouteBuilder UsersService.RouteBuilder
    |> Remoting.withBaseUrl baseUrl
    |> Remoting.withAuthorizationHeader (sprintf "Bearer %s" token)
    |> Remoting.buildProxy<UsersService>
    |> fn
    