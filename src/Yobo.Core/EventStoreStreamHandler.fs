module Yobo.Core.EventStoreStreamHandler

open System
open Newtonsoft.Json.Linq
open Yobo.Shared.Validation
open Yobo.Shared.Domain
open FSharp.Rop
open CosmoStore
open FSharp.Control.Tasks.V2

type EventStoreError = 
    | General of Exception

type Aggregate<'state, 'command, 'event> = {
    Init : 'state
    Apply: 'state -> 'event -> 'state
    Execute: 'state -> 'command -> Result<'event list, DomainError>
}

type StreamHandlerError =
    | EventStoreError of EventStoreError
    | DomainError of DomainError
    | ValidationError of ValidationError list

type StreamHandler<'state, 'command, 'event> = {
    GetCurrentState : string -> Result<'state, EventStoreError>
    GetCurrentStateWithNextPosition : string -> Result<'state * int64, EventStoreError>
    HandleCmd : 'command -> Result<'event list, StreamHandlerError>
    HandleCorrelatedCmd : Guid -> 'command -> Result<'event list, StreamHandlerError>
}

// Ideas:
// 1. What about to make Domain error one of Validation errors?
// 2. Validators to get state?


//type TODO<'cmd,'event> =
//    | Direct of 'cmd
//    | Compensating of command:'cmd * onSuccess:(unit -> Result<_, StreamHandlerError>) * compesation:'event

type EventSerializer<'event> = {
    EventToData : 'event -> string * JToken
    DataToEvent : string * JToken -> 'event
}

type StreamHandlerSettings<'state, 'command, 'event> = {
    Aggregate : Aggregate<'state, 'command, 'event>
    GetStreamId : 'command -> string
    Serializer : EventSerializer<'event>
    Validators : ('command -> Result<'command, ValidationError list>) list
}

let private runValidators validators cmd =
    let foldFn acc item =
        match cmd |> item with
        | Error e -> acc @ e
        | Ok _ -> acc
    validators |> List.fold foldFn []

let private toEventWrite corrId (name, data) =
    let id = Guid.NewGuid()
    {
        Id = id
        CorrelationId = defaultArg corrId id
        Name = name
        Data = data
        Metadata = None
    }

let private getCurrentStateAndPosition<'state, 'event> (toEvent:string * JToken -> 'event) (init:'state) (apply:'state -> 'event -> 'state) (eventStore:EventStore) streamId = 
    task {
        try
            let toEventFn (e:EventRead) = toEvent(e.Name, e.Data)
            let! events = eventStore.GetEvents streamId EventsReadRange.AllEvents
            let expectedPosition = 
                events 
                |> List.map (fun x -> x.Position) 
                |> List.tryLast |> function | None -> 1L | Some p -> p + 1L
            let domainEvents = events |> List.map toEventFn
            let currentState = domainEvents |> List.fold apply init
            return Ok (currentState,expectedPosition)
        with ex -> return (EventStoreError.General(ex) |> Error)
    }

let getStreamHandler<'state, 'command, 'event> (settings:StreamHandlerSettings<'state, 'command, 'event>) (eventStore:EventStore) =
    // state reader
    let getCurrentStateFn = getCurrentStateAndPosition settings.Serializer.DataToEvent settings.Aggregate.Init settings.Aggregate.Apply eventStore

    // handle function
    let handleCmd corrId cmd =
        task {
            try
                match cmd |> runValidators settings.Validators with
                | [] ->
                    let streamId = cmd |> settings.GetStreamId
                    let! currentStateAndPosition = streamId |> getCurrentStateFn
                    match (currentStateAndPosition |> Result.mapError EventStoreError) with
                    | Ok (state,position) ->
                        match cmd |> settings.Aggregate.Execute state with
                        | Ok newEvents ->
                            let! _ = newEvents |> List.map (settings.Serializer.EventToData >> toEventWrite corrId) |> eventStore.AppendEvents streamId (Exact position)
                            return Ok newEvents

                        | Error e -> return e |> StreamHandlerError.DomainError |> Error
                    | Error e -> return e |> Error
                | errors -> return ValidationError(errors) |> Error
            with ex -> return (EventStoreError.General(ex) |> EventStoreError |> Error)
        }
    
    {
        GetCurrentStateWithNextPosition = getCurrentStateFn >> Async.AwaitTask >> Async.RunSynchronously
        GetCurrentState = getCurrentStateFn >> Async.AwaitTask >> Async.RunSynchronously >> Result.map fst
        HandleCmd = fun cmd -> handleCmd None cmd |> Async.AwaitTask |> Async.RunSynchronously
        HandleCorrelatedCmd = fun corrId cmd -> handleCmd (Some corrId) cmd |> Async.AwaitTask |> Async.RunSynchronously
    }

module Extractor =
    let inline extractId x = (^T:(member Id:Guid)x)
    let formatId (i:Guid) = i.ToString("N")
    let inline getIdFromCommand x =
        x 
        |> extractId
        |> formatId