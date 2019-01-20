module Yobo.Core.Users.CommandHandler

open Yobo.Core.EventStoreCommandHandler
open Yobo.Core.Users
open Yobo.Libraries.Security.SymetricCryptoProvider

let private getId = function
    | Register args -> args |> Extractor.getIdFromCommand
    | RegenerateActivationKey args -> args |> Extractor.getIdFromCommand
    | Activate args -> args |> Extractor.getIdFromCommand
    | AddCredits args -> args |> Extractor.getIdFromCommand
    | WithdrawCredits args -> args |> Extractor.getIdFromCommand
    | RefundCredits args -> args |> Extractor.getIdFromCommand
    | BlockCashReservations args -> args |> Extractor.getIdFromCommand
    | UnblockCashReservations args -> args |> Extractor.getIdFromCommand

let private settings cryptoProvider = {
    Aggregate = {
        Init = State.Init
        Execute = Aggregate.execute
        Apply = Aggregate.apply
    }
    GetStreamId = getId >> sprintf "%s%s" EventSerializer.streamPrefix
    Serializer = {
        EventToData = EventSerializer.toData cryptoProvider
        DataToEvent = EventSerializer.toEvent cryptoProvider
    }
    Validators = [ ]
    RollbackEvents =
        fun state cmd ->
            match cmd with
            | WithdrawCredits args -> CreditsRefunded { Id = args.Id; Amount = args.Amount; LessonId = args.LessonId } |> List.singleton
            | RefundCredits args -> CreditsWithdrawn { Id = args.Id; Amount = args.Amount; LessonId = args.LessonId } |> List.singleton
            | BlockCashReservations args -> CashReservationsUnblocked { Id = args.Id } |> List.singleton
            | UnblockCashReservations args ->
                if state.LastCashBlockingDate.IsSome then
                    CashReservationsBlocked { Id = args.Id; Expires = state.LastCashBlockingDate.Value } |> List.singleton
                else []
            | _ -> []
}

let private cmdBuilder _ = function
    | Register args -> Registry.Add { UserId = args.Id; Email = args.Email } |> Some
    | _ -> None

let get (cryptoProvider:SymetricCryptoProvider) store =
    let registryHandler = store |> Registry.CommandHandler.get
    store |> getRollbackCommandHandler (settings cryptoProvider) registryHandler cmdBuilder