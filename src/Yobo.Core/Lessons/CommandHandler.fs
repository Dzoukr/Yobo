module Yobo.Core.Lessons.CommandHandler

open Yobo.Core.EventStoreCommandHandler

let private getId = function
    | Create args -> args |> Extractor.getIdFromCommand

let private settings = {
    Aggregate = {
        Init = State.Init
        Execute = Aggregate.execute
        Apply = Aggregate.apply
    }
    GetStreamId = getId >> sprintf "Lessons-%s"
    Serializer = {
        EventToData = EventSerializer.toData
        DataToEvent = EventSerializer.toEvent
    }
    Validators = [ ]
}

let get = getCommandHandler settings