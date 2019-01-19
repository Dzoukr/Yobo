module Yobo.Core.Users.Registry.CommandHandler

open Yobo.Core.EventStoreCommandHandler

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
    RollbackEvents =
        fun _ cmd ->
            match cmd with
            | Add args -> Removed { UserId = args.UserId; Email = args.Email } |> List.singleton
            | _ -> []
}

let get = getCommandHandler settings