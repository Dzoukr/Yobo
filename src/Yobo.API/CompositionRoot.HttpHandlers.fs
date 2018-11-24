module Yobo.API.CompositionRoot.HttpHandlers

open Giraffe
open Yobo.Shared.Communication
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2


let private toHandler res =
    let code err =
        match err with
        | ServerError.ValidationError _ -> RequestErrors.BAD_REQUEST
        | ServerError.DomainError _ -> RequestErrors.BAD_REQUEST
        | ServerError.Exception _ -> ServerErrors.INTERNAL_ERROR

    match res with
    | Ok r -> r |> Successful.OK
    | Error e -> e |> code e

let tryBindJson<'T> (errorF:System.Exception -> HttpHandler) (f : 'T -> HttpHandler) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            try
                let! model = ctx.BindJsonAsync<'T>()
                return! f model next ctx
            with ex -> return! errorF ex next ctx
        }

let safeBindJson<'a> = tryBindJson (ServerError.Exception >> Error >> toHandler)

module Login =
    open Yobo.API.Login.HttpHandlers
    open Yobo.Shared.Login.Register.Domain
    // TODO: Add hash fn!!!
    let register : HttpHandler = safeBindJson<Account> (register Services.CommandHandler.handle (fun x -> x) >> toHandler)