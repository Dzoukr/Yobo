module Yobo.API.Pipeline

open System
open Yobo.Core.CQRS
open Yobo.Shared.Communication
open Configuration
open Yobo.Core

type EventHandler = CoreEvent -> unit
type CommandHandler = {
    HandleForUser : Guid -> CoreCommand -> Result<CoreEvent list, ServerError>
    HandleAnonymous : CoreCommand -> Result<CoreEvent list, ServerError>
}
    

open Yobo.Core.Users.EventSerializer
open Yobo.Core.Lessons.EventSerializer
open Yobo.Core.Workshops.EventSerializer
open Yobo.Core.Metadata
open Yobo.Core.EventStoreCommandHandler

let getEventHandler (conf:ApplicationConfiguration) (svc:Services.Services) =
    let emailSettings : EmailSettings.Settings = { From = conf.Emails.From; BaseUrl = conf.Server.BaseUrl }
    let dbHandleFn = DbEventHandler.getHandler conf.ReadDbConnectionString
    let emailHandleFn = EmailEventHandler.getHandler svc.Users.ReadQueries svc.Emails emailSettings
    let handle evn =
        evn |> emailHandleFn |> ignore
        evn |> dbHandleFn |> ignore
    
    function
        | LessonsEvent evn -> evn |> CoreEvent.Lessons |> handle
        | WorkshopsEvent evn -> evn |> CoreEvent.Workshops |> handle
        | UsersEvent svc.SymetricCryptoProvider evn -> evn |> CoreEvent.Users |> handle
        | _ -> ()

let private cmdHandlerErrorToServerError = function
    | CommandHandlerError.EventStoreError (General ex) -> ServerError.Exception(ex.Message)
    | CommandHandlerError.DomainError err -> ServerError.DomainError(err)
    | CommandHandlerError.ValidationError err -> ServerError.ValidationError(err)

let getCommandHandler (svc:Services.Services) =
    let handleFn = CommandHandler.getHandler svc.SymetricCryptoProvider svc.EventStore
    let handle (meta:Metadata) cmd = cmd |> handleFn meta (Guid.NewGuid()) |> Result.mapError cmdHandlerErrorToServerError
    {
        HandleForUser = fun userId -> handle (Metadata.Create userId)
        HandleAnonymous = handle (Metadata.CreateAnonymous())
    }