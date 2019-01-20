module Yobo.Core.Lessons.CommandHandler

open Yobo.Core
open Yobo.Core.EventStoreCommandHandler

let private getId = function
    | Create args -> args |> Extractor.getIdFromCommand
    | AddReservation args -> args |> Extractor.getIdFromCommand
    | CancelReservation args -> args |> Extractor.getIdFromCommand

let private settings = {
    Aggregate = {
        Init = State.Init
        Execute = Aggregate.execute
        Apply = Aggregate.apply
    }
    GetStreamId = getId >> sprintf "%s%s" EventSerializer.streamPrefix
    Serializer = {
        EventToData = EventSerializer.toData
        DataToEvent = EventSerializer.toEvent
    }
    Validators = []
    RollbackEvents = fun _ _ -> []
}

let private cmdBuilder (state:State) = function
    | AddReservation args ->
        if args.UseCredits then
            Users.WithdrawCredits { Id = args.UserId; Amount = args.Count; LessonId = args.Id } |> Some
        else
            Users.BlockCashReservations { Id = args.UserId; Expires = state.EndDate } |> Some
    | CancelReservation args ->
        state.Reservations
        |> List.tryFind (fun (x,_,_) -> x = args.UserId)
        |> Option.map (fun (u,c,useCredits) ->
            if useCredits then
                Users.RefundCredits { Id = u; Amount = c; LessonId = args.Id }
            else Users.UnblockCashReservations { Id = u }
        )
    | _ -> None

let get usersHandler store =
    store |> getRollbackCommandHandler settings usersHandler cmdBuilder