module Yobo.Core.EventStoreCommandHandler

open System
open Newtonsoft.Json.Linq
open Yobo.Shared
open Yobo.Shared.Validation
open Yobo.Shared.Domain
open FSharp.Rop
open CosmoStore
open FSharp.Control.Tasks
open Yobo.Core.Metadata

type EventStoreError = 
    | General of Exception

type Aggregate<'state, 'command, 'event> = {
    Init : 'state
    Apply: 'state -> 'event -> 'state
    Execute: 'state -> 'command -> Result<'event list, DomainError>
}

type CommandHandlerError =
    | EventStoreError of EventStoreError
    | DomainError of DomainError
    | ValidationError of ValidationError list

type EventSerializer<'event> = {
    EventToData : 'event -> string * JToken
    DataToEvent : string * JToken -> 'event
}

type StreamIdReader<'command, 'event> = {
    FromCommand : 'command -> string
    FromEvent : 'event -> string
}

type CommandHandlerSettings<'state, 'command, 'event> = {
    Aggregate : Aggregate<'state, 'command, 'event>
    StreamIdReader : StreamIdReader<'command, 'event>
    Serializer : EventSerializer<'event>
    Validators : ('command -> Result<'command, ValidationError list>) list
    TryGetRollbackEvent : 'state -> 'event -> 'event option
}

type CommandHandler<'state, 'command, 'event> = {
    HandleCommand : Metadata -> Guid -> 'command -> Result<'event list, CommandHandlerError>
    CompensateEvent : Metadata -> Guid -> 'event -> Result<unit, CommandHandlerError>
    GetState : 'command -> Result<'state, CommandHandlerError>
}

let private runValidators validators cmd =
    let foldFn acc item =
        match cmd |> item with
        | Error e -> acc @ e
        | Ok _ -> acc
    validators
    |> List.fold foldFn []
    |> function
        | [] -> Ok cmd
        | errs -> ValidationError(errs) |> Error

let private toEventWrite metadata corrId (name, data) =
    {
        Id = Guid.NewGuid()
        CorrelationId = corrId
        Name = name
        Data = data
        Metadata = metadata |> Serialization.objectToJToken |> Some
    }

let private getCurrentStateAndPosition<'state, 'event> (toEvent:string * JToken -> 'event) (init:'state) (apply:'state -> 'event -> 'state) (eventStore:EventStore) streamId = 
    task {
        let toEventFn (e:EventRead) = toEvent(e.Name, e.Data)
        let! events = eventStore.GetEvents streamId EventsReadRange.AllEvents
        let expectedPosition = 
            events 
            |> List.map (fun x -> x.Position) 
            |> List.tryLast |> function | None -> 1L | Some p -> p + 1L
        let domainEvents = events |> List.map toEventFn
        let currentState = domainEvents |> List.fold apply init
        return (currentState,expectedPosition)
    }

let getCommandHandler<'state,'command, 'event>
    (settings:CommandHandlerSettings<'state, 'command, 'event>)
    (eventStore:EventStore) : CommandHandler<'state, 'command, 'event> =

    // state reader
    let getCurrentStateFn = getCurrentStateAndPosition settings.Serializer.DataToEvent settings.Aggregate.Init settings.Aggregate.Apply eventStore

    let compensateEvent meta corrId (evn:'event) =
        task {
            try
                let streamId = evn |> settings.StreamIdReader.FromEvent
                let! state,_ = streamId |> getCurrentStateFn
                match evn |> settings.TryGetRollbackEvent state with
                | None -> return Ok ()
                | Some e ->
                    let! _ = e |> settings.Serializer.EventToData |> toEventWrite meta corrId |> eventStore.AppendEvent streamId Any
                    return Ok ()
            with ex -> return (EventStoreError.General(ex) |> EventStoreError |> Error)
        }

    let handleCmd meta corrId (cmd:'command) =
        task {
            try
                match cmd |> runValidators settings.Validators with
                | Ok cmd ->
                    let streamId = cmd |> settings.StreamIdReader.FromCommand
                    let! state,position = streamId |> getCurrentStateFn
                    match cmd |> settings.Aggregate.Execute state with
                    | Ok newEvents ->
                        let! _ = newEvents |> List.map (settings.Serializer.EventToData >> toEventWrite meta corrId) |> eventStore.AppendEvents streamId (Exact position)
                        return Ok newEvents
                    | Error e -> return e |> CommandHandlerError.DomainError |> Error
                | Error e -> return e |> Error
            with ex -> return (EventStoreError.General(ex) |> EventStoreError |> Error)
        }

    let getState c =
        task {
            try
                let! s,_ = c |> settings.StreamIdReader.FromCommand |> getCurrentStateFn
                return Ok s
            with ex -> return (EventStoreError.General(ex) |> EventStoreError |> Error)
        }


    {
        HandleCommand = fun m c -> handleCmd m c >> Async.AwaitTask >> Async.RunSynchronously
        CompensateEvent = fun m c -> compensateEvent m c >> Async.AwaitTask >> Async.RunSynchronously
        GetState = getState >> Async.AwaitTask >> Async.RunSynchronously
    }



module Extractor =
    let inline extractId x = (^T:(member Id:Guid)x)
    let formatId (i:Guid) = i.ToString("N")
    let inline getIdFromCommand x =
        x 
        |> extractId
        |> formatId