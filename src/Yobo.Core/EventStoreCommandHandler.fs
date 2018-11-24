module Yobo.Core.EventStoreCommandHandler

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

type CommandHandlerError =
    | EventStoreError of EventStoreError
    | DomainError of DomainError
    | ValidationError of ValidationError list

type EventSerializer<'event> = {
    EventToData : 'event -> string * JToken
    DataToEvent : string * JToken -> 'event
}

type CommandHandlerSettings<'state, 'command, 'event> = {
    Aggregate : Aggregate<'state, 'command, 'event>
    GetStreamId : 'command -> string
    Serializer : EventSerializer<'event>
    Validators : ('command -> Result<'command, ValidationError list>) list
}

type CompensatingCommandHandlerSettings<'state, 'command, 'event, 'outerCommand, 'outerEvent> = {
    Aggregate : Aggregate<'state, 'command, 'event>
    GetStreamId : 'command -> string
    Serializer : EventSerializer<'event>
    Validators : ('command -> Result<'command, ValidationError list>) list
    CompensationBuilder : 'outerCommand -> 'command * 'event list
    OuterCommandHandler : Guid option -> 'outerCommand -> Result<'outerEvent list, CommandHandlerError>
}

let private runValidators validators cmd =
    let foldFn acc item =
        match cmd |> item with
        | Error e -> acc @ e
        | Ok _ -> acc
    validators |> List.fold foldFn []

let private toEventWrite corrId (name, data) =
    {
        Id = Guid.NewGuid()
        CorrelationId = corrId
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


let getCompensatingCommandHandler<'state, 'command, 'event, 'outerCommand, 'outerEvent> 
    (settings:CompensatingCommandHandlerSettings<'state, 'command, 'event, 'outerCommand, 'outerEvent>) 
    (eventStore:EventStore) =
    // state reader
    let getCurrentStateFn = getCurrentStateAndPosition settings.Serializer.DataToEvent settings.Aggregate.Init settings.Aggregate.Apply eventStore

    // handle function
    let handleCmd corrId (outerCmd:'outerCommand) =
        task {
            try
                let corrId = defaultArg corrId (Guid.NewGuid())
                let cmd, compensationEvents = outerCmd |> settings.CompensationBuilder
                match cmd |> runValidators settings.Validators with
                | [] ->
                    let streamId = cmd |> settings.GetStreamId
                    let! currentStateAndPosition = streamId |> getCurrentStateFn
                    match (currentStateAndPosition |> Result.mapError EventStoreError) with
                    | Ok (state,position) ->
                        match cmd |> settings.Aggregate.Execute state with
                        | Ok newEvents ->
                            let! _ = newEvents |> List.map (settings.Serializer.EventToData >> toEventWrite corrId) |> eventStore.AppendEvents streamId (Exact position)
                            match settings.OuterCommandHandler (Some corrId) outerCmd with
                            | Ok subEvents ->                           
                                return Ok subEvents
                            | Error e ->
                                let newPosition = position + int64 newEvents.Length
                                let! _ = compensationEvents |> List.map (settings.Serializer.EventToData >> toEventWrite corrId) |> eventStore.AppendEvents streamId (Exact newPosition)
                                return Error e
                        | Error e -> return e |> CommandHandlerError.DomainError |> Error
                    | Error e -> return e |> Error
                | errors -> return ValidationError(errors) |> Error
            with ex -> return (EventStoreError.General(ex) |> EventStoreError |> Error)
        }
    fun corrId -> handleCmd corrId >> Async.AwaitTask >> Async.RunSynchronously

let getCommandHandler<'state, 'command, 'event> (settings:CommandHandlerSettings<'state, 'command, 'event>) (eventStore:EventStore) =
    // state reader
    let getCurrentStateFn = getCurrentStateAndPosition settings.Serializer.DataToEvent settings.Aggregate.Init settings.Aggregate.Apply eventStore

    // handle function
    let handleCmd corrId cmd =
        task {
            try
                let corrId = defaultArg corrId (Guid.NewGuid())
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
                        | Error e -> return e |> CommandHandlerError.DomainError |> Error
                    | Error e -> return e |> Error
                | errors -> return ValidationError(errors) |> Error
            with ex -> return (EventStoreError.General(ex) |> EventStoreError |> Error)
        }
    fun corrId -> handleCmd corrId >> Async.AwaitTask >> Async.RunSynchronously


module Extractor =
    let inline extractId x = (^T:(member Id:Guid)x)
    let formatId (i:Guid) = i.ToString("N")
    let inline getIdFromCommand x =
        x 
        |> extractId
        |> formatId