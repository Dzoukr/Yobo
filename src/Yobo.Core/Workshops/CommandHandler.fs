module Yobo.Core.Workshops.CommandHandler

open Yobo.Core.EventStoreCommandHandler

let private getIdFromCmd = function
    | Create args -> args |> Extractor.getIdFromCommand
    | Delete args -> args |> Extractor.getIdFromCommand

let private getIdFromEvn = function
    | Created args -> args |> Extractor.getIdFromCommand
    | Deleted args -> args |> Extractor.getIdFromCommand


let private settings = {
    Aggregate = {
        Init = State.Init
        Execute = Aggregate.execute
        Apply = Aggregate.apply
    }
    StreamIdReader = {
        FromCommand = getIdFromCmd >> sprintf "%s%s" EventSerializer.streamPrefix
        FromEvent = getIdFromEvn >> sprintf "%s%s" EventSerializer.streamPrefix
    }
    Serializer = {
        EventToData = EventSerializer.toData
        DataToEvent = EventSerializer.toEvent
    }
    Validators = []
    TryGetRollbackEvent = fun _ _ -> None
}

let get store =
    store |> getCommandHandler settings