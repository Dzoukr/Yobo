module Yobo.Core.Users.Registry.CommandHandler

open Yobo.Core.EventStoreCommandHandler

let private settings compensationBuilder outerCmdHandler = {
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
    OuterCommandHandler = outerCmdHandler
    CompensationBuilder = compensationBuilder
}

let get compensationBuilder outerCmdHandler = getCompensatingCommandHandler (settings compensationBuilder outerCmdHandler)