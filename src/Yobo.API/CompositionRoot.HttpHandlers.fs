module Yobo.API.CompositionRoot.HttpHandlers

open System
open Giraffe
open Yobo.Shared.Communication
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2

let private toHandler = function
    | Ok v -> Successful.OK v
    | Error e ->
        let code =
            match e with
            | ServerError.Exception _ -> ServerErrors.INTERNAL_ERROR
            | _ -> RequestErrors.BAD_REQUEST
        e |> code
    
let private tryBindJson<'T> (errorF:System.Exception -> HttpHandler) (f : 'T -> HttpHandler) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            try
                let! model = ctx.BindJsonAsync<'T>()
                return! f model next ctx
            with ex -> return! errorF ex next ctx
        }

let private safeBindJson<'a> = tryBindJson<'a>((fun ex -> ex.Message) >> ServerError.Exception >> Error >> toHandler)

module Registration =
    open Yobo.API.Registration.HttpHandlers
    open Yobo.Shared.Registration.Domain
    open Yobo.Libraries.Security
    open Yobo.API.CompositionRoot

    let register : HttpHandler = safeBindJson<Account> (register Services.CommandHandler.handle Password.createHash >> toHandler)
    let activateAccount _ = safeBindJson<Guid> (activateAccount Services.CommandHandler.handle Services.Users.queries.GetByActivationKey >> toHandler)

module Login =
    open Yobo.API.Login.HttpHandlers
    open Yobo.Shared.Login.Domain
    open Yobo.API.CompositionRoot
    open Yobo.Libraries.Security
    
    let login : HttpHandler = safeBindJson<Login> (login (Services.Users.authenticator.Login Password.verifyPassword) >> toHandler)
