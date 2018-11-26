module Yobo.API.CompositionRoot.Services

open Yobo.Core
open Yobo.Core.EventStoreCommandHandler
open Yobo.Shared.Communication
open Yobo.Libraries.Security
open Yobo.Libraries.Emails
open FSharp.Rop
open Yobo.API

// services
let private eventStore = Configuration.EventStore.get |> CosmoStore.TableStorage.EventStore.getEventStore
let private cryptoProvider = Configuration.SymetricCryptoProvide.get |> TableStorageSymetricCryptoProvider.create
let private mailService : EmailProvider = Configuration.Emails.Mailjet.get |> MailjetProvider.create
let private mailFrom : Address = Configuration.Emails.from

// error handling
let private toServerError = function
    | CommandHandlerError.EventStoreError (General ex) -> ServerError.Exception(ex.Message)
    | CommandHandlerError.DomainError err -> ServerError.DomainError(err)
    | CommandHandlerError.ValidationError err -> ServerError.ValidationError(err)

// event handlers
module EventHandler =
    let private dbHandleFn = DbEventHandler.getHandler Configuration.ReadDb.get
    let private emailHandleFn = EmailEventHandler.getHandler mailService mailFrom
    let handle evn =
        evn |> dbHandleFn |> ignore
        evn |> emailHandleFn |> ignore
        evn

// command handler
module CommandHandler =

    let private handleFn = CommandHandler.getHandler cryptoProvider eventStore
    let handle cmd =
        cmd |> handleFn
        <!> List.map EventHandler.handle
        |> Result.mapError toServerError
