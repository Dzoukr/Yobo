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
open Yobo.Server
open Yobo.Shared.Domain
open Yobo.Shared.UserAccount.Communication

let private userAccountService validator (root:CompositionRoot) : UserAccountService =
    {
        GetUserInfo =
            validator
            >> TaskResult.ofResult
            >> TaskResult.map (fun (i,p) -> { Id = i; FirstName = "AAA"; LastName = "BBB" } : Yobo.Shared.UserAccount.Communication.Response.UserInfo)
            >> Async.AwaitTask
    }

let userAccountServiceHandler (root:CompositionRoot) : HttpHandler =
    
    let validator (sp:SecuredParam<_>) =
        match root.Auth.ValidateToken sp.Token with
        | Some claims ->
            let userId =
                claims
                |> Seq.find (fun x -> x.Type = "Id")
                |> fun x -> x.Value
                |> Guid
            Ok (userId, sp.Parameter)
        | None -> InvalidOrExpiredToken |> Authentication |> Error
    
    Remoting.createApi()
    |> Remoting.withRouteBuilder UserAccountService.RouteBuilder
    |> Remoting.fromValue (userAccountService validator root)
    |> Remoting.buildHttpHandler