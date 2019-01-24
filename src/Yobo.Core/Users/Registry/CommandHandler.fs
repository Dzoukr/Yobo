module Yobo.Core.Users.Registry.CommandHandler

open Yobo.Core.EventStoreCommandHandler

let private settings = {
    Aggregate = {
        Init = State.Init
        Execute = Aggregate.execute
        Apply = Aggregate.apply
    }
    StreamIdReader = {
        FromEvent = (fun _ -> sprintf "UserEmails")
        FromCommand = (fun _ -> sprintf "UserEmails")
    }
    Serializer = {
        EventToData = EventSerializer.toData
        DataToEvent = EventSerializer.toEvent
    }
    Validators = [ CommandValidator.validate ]
    TryGetRollbackEvent =
        fun _ evn ->
            match evn with
            | Added args -> Removed { UserId = args.UserId; Email = args.Email } |> Some
            | _ -> None
}

let get = getCommandHandler settings