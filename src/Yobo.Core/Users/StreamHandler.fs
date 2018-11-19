module Yobo.Core.Users.StreamHandler

open Yobo.Core.EventStoreStreamHandler

let private getId = function
    | Register args -> args |> Extractor.getIdFromCommand

let private settings = {
    Aggregate = {
        Init = State.Init
        Execute = Aggregate.execute
        Apply = Aggregate.apply
    }
    GetStreamId = getId >> sprintf "Users-%s"
    Serializer = {
        EventToData = EventSerializer.toData
        DataToEvent = EventSerializer.toEvent
    }
    Validators = [ ]
}

let doit cmd =
    match cmd with
    | Register args ->
        let before = ({ UserId = args.Id; Email = args.Email } : Registry.CmdArgs.Add) |> Registry.Command.Add
        let onError = ({ UserId = args.Id; Email = args.Email } : Registry.CmdArgs.Remove) |> Registry.Event.Removed |> List.singleton
        Some (before, onError)
    | _ -> None

//let intercept (handler:StreamHandler<State,Command,Event>) =
    


let getHandler eventStore =
    let registryHandler = Registry.StreamHandler.getHandler eventStore
    
    getStreamHandler settings