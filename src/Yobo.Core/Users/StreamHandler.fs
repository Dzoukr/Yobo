module Yobo.Core.Users.StreamHandler

open Yobo.Core.EventStoreStreamHandler

let private getId = function
    | Create args -> args |> Extractor.getIdFromCommand

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

let getHandler = getStreamHandler settings