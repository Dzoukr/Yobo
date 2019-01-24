module Yobo.Core.Lessons.CommandHandler

open Yobo.Core.EventStoreCommandHandler

let private getIdFromCmd = function
    | Create args -> args |> Extractor.getIdFromCommand
    | AddReservation args -> args |> Extractor.getIdFromCommand
    | CancelReservation args -> args |> Extractor.getIdFromCommand
    | Cancel args -> args |> Extractor.getIdFromCommand

let private getIdFromEvn = function
    | Created args -> args |> Extractor.getIdFromCommand
    | ReservationAdded args -> args |> Extractor.getIdFromCommand
    | ReservationCancelled args -> args |> Extractor.getIdFromCommand
    | Cancelled args -> args |> Extractor.getIdFromCommand
    | Reopened args -> args |> Extractor.getIdFromCommand


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
    TryGetRollbackEvent =
        fun _ evn ->
            match evn with
            | Cancelled args -> Reopened { Id = args.Id } |> Some
            | _ -> None
}

let get store =
    store |> getCommandHandler settings