module Yobo.API.CompositionRoot.Services

open System
open Yobo.Core
open Yobo.Core.EventStoreCommandHandler
open Yobo.Shared.Communication
open Yobo.Libraries.Security
open Yobo.Libraries.Emails
open FSharp.Rop
open Yobo.API
open Yobo.Core.Data
open Yobo.Shared.Domain

// services
let private eventStore = Configuration.EventStore.get |> CosmoStore.TableStorage.EventStore.getEventStore
let private cryptoProvider = Configuration.SymetricCryptoProvider.get |> TableStorageSymetricCryptoProvider.create
let private emailService : EmailProvider = Configuration.Emails.Mailjet.get |> MailjetProvider.create
let private emailSettings : EmailSettings.Settings = { From = Configuration.Emails.from; BaseUrl = Configuration.Server.baseUrl }

// error handling
let private cmdHandlerErrorToServerError = function
    | CommandHandlerError.EventStoreError (General ex) -> ServerError.Exception(ex.Message)
    | CommandHandlerError.DomainError err -> ServerError.DomainError(err)
    | CommandHandlerError.ValidationError err -> ServerError.ValidationError(err)

let private dbErrorToServerError = function
    | DbError.ItemNotFound id -> DomainError.ItemDoesNotExist "Id" |> ServerError.DomainError
    | DbError.Exception e -> e.Message |> ServerError.Exception

module Users =
    let queries =
        Configuration.ReadDb.connectionString
        |> Users.ReadQueries.createDefault 
        |> Users.ReadQueries.withError dbErrorToServerError

    let authenticator =
        Password.verifyPassword
        |> Users.Authenticator.createDefault Configuration.ReadDb.connectionString
        |> Users.Authenticator.withError ServerError.AuthError

    let authorizator = Configuration.Authorization.get |> Yobo.Libraries.Authorization.Jwt.createAuthorizator

module Lessons =
    let queries =
        Configuration.ReadDb.connectionString
        |> Lessons.ReadQueries.createDefault
        |> Lessons.ReadQueries.withError dbErrorToServerError

// event handlers
module EventHandler =
    let private dbHandleFn = DbEventHandler.getHandler Configuration.ReadDb.connectionString
    let private emailHandleFn = EmailEventHandler.getHandler Users.queries emailService emailSettings
    let handle evn =
        evn |> dbHandleFn |> ignore
        evn |> emailHandleFn |> ignore
        evn

// command handler
module CommandHandler =
    open Yobo.Core.Metadata

    let private handleFn : CommandHandler<CoreCommand, CoreEvent> = CommandHandler.getHandler cryptoProvider eventStore
    let handle (meta:Metadata) cmd =
        cmd |> handleFn meta (Guid.NewGuid())
        <!> List.map EventHandler.handle
        |> Result.mapError cmdHandlerErrorToServerError
    let handleAnonymous = handle (Metadata.CreateAnonymous())
    let handleForUser userId = handle (Metadata.Create userId)
