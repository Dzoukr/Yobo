module Yobo.Core.Users.Registry.StreamHandler

open Yobo.Core.EventStoreStreamHandler

let private settings = {
    Aggregate = {
        Init = State.Init
        Execute = Aggregate.execute
        Apply = Aggregate.apply
    }
    GetStreamId = (fun _ -> sprintf "UserEmails")
    Serializer = {
        EventToData = EventSerializer.toData
        DataToEvent = EventSerializer.toEvent
    }
    Validators = [ CommandValidator.validate ]
}

let getHandler = getStreamHandler settings