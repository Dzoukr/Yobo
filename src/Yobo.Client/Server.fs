module Yobo.Client.Server

open System
open Fable.Core
open Fable.Core
open Fable.Core.JsInterop
open Fable.Remoting.Client
open Fable.SimpleJson
open Fable.SimpleJson
open Fable.SimpleJson
open Yobo.Shared.Auth.Communication
open Yobo.Shared.UserAccount.Communication
open Yobo.Shared.Domain

let exnToError (e:exn) : ServerError =
    match e with  
    | :? Fable.Remoting.Client.ProxyRequestException as ex -> 
        let body : obj = JS.JSON.parse ex.Response.ResponseBody
        let errorString : string = JS.JSON.stringify body?error
        Json.parseAs<ServerError>(errorString)
    | _ -> (ServerError.Exception(e.Message))

module Cmd =
    open Elmish
    
    module OfAsync =
        let eitherResult f args resultMsg =
            let onError (ex:exn) = ex.Message |> ServerError.Exception |> Error |> resultMsg 
            Cmd.OfAsync.either f args resultMsg onError

module SecuredParam =
    let create p =
        {
            Token = TokenStorage.tryGetToken() |> Option.defaultValue ""
            Parameter = p
        }

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
    