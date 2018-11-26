module Yobo.API.CompositionRoot.HttpHandlers

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

let private safeBindJson<'a> = tryBindJson ((fun ex -> ex.Message) >> ServerError.Exception >> Error >> toHandler)

module Registration =
    open Yobo.API.Registration.HttpHandlers
    open Yobo.Shared.Registration.Domain
    open Yobo.Libraries.Security

    let register : HttpHandler = safeBindJson<Account> (register Services.CommandHandler.handle Password.createHash >> toHandler)