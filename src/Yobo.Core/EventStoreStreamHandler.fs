module Yobo.Core.EventStoreStreamHandler

open System
open Newtonsoft.Json.Linq
open Yobo.Shared.Validation

type EventStoreError = 
    | General of Exception

type DomainError = TODO

type Aggregate<'state, 'command, 'event> = {
    Init : 'state
    Apply: 'state -> 'event -> 'state
    Execute: 'state -> 'command -> Result<'event list, DomainError>
}

type StreamHandlerError =
    | EventStoreError of EventStoreError
    | DomainError of DomainError
    | ValidationError of ValidationError

type StreamHandler<'state, 'command, 'event> = {
    GetCurrentState : string -> Result<'state, EventStoreError>
    GetCurrentStateWithNextPosition : string -> Result<'state * int64, EventStoreError>
    HandleCmd : 'command -> Result<'event list, StreamHandlerError>
    HandleCorrelatedCmd : Guid -> 'command -> Result<'event list, StreamHandlerError>
}

type EventSerializer<'event> = {
    EventToData : 'event -> string * JToken
    DataToEvent : string * JToken -> 'event
}

type StreamHandlerSettings<'state, 'command, 'event> = {
    Aggregate : Aggregate<'state, 'command, 'event>
    GetStreamId : 'command -> string
    Serializer : EventSerializer<'event>
    Validators : ('command -> Result<'command, ValidationError>) list
}