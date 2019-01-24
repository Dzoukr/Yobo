module Yobo.Core.Users.CommandHandler

open Yobo.Core.EventStoreCommandHandler
open Yobo.Core.Users
open Yobo.Libraries.Security.SymetricCryptoProvider

let private getIdFromCmd = function
    | Register args -> args |> Extractor.getIdFromCommand
    | RegenerateActivationKey args -> args |> Extractor.getIdFromCommand
    | Activate args -> args |> Extractor.getIdFromCommand
    | AddCredits args -> args |> Extractor.getIdFromCommand
    | WithdrawCredits args -> args |> Extractor.getIdFromCommand
    | RefundCredits args -> args |> Extractor.getIdFromCommand
    | BlockCashReservations args -> args |> Extractor.getIdFromCommand
    | UnblockCashReservations args -> args |> Extractor.getIdFromCommand

let private getIdFromEvn = function
    | Registered args -> args |> Extractor.getIdFromCommand
    | ActivationKeyRegenerated args -> args |> Extractor.getIdFromCommand
    | Activated args -> args |> Extractor.getIdFromCommand
    | CreditsAdded args -> args |> Extractor.getIdFromCommand
    | CreditsWithdrawn args -> args |> Extractor.getIdFromCommand
    | CreditsRefunded args -> args |> Extractor.getIdFromCommand
    | CashReservationsBlocked args -> args |> Extractor.getIdFromCommand
    | CashReservationsUnblocked args -> args |> Extractor.getIdFromCommand

let private settings cryptoProvider = {
    Aggregate = {
        Init = State.Init
        Execute = Aggregate.execute
        Apply = Aggregate.apply
    }
    StreamIdReader = {
        FromEvent = getIdFromEvn >> sprintf "%s%s" EventSerializer.streamPrefix
        FromCommand = getIdFromCmd >> sprintf "%s%s" EventSerializer.streamPrefix
    }
    Serializer = {
        EventToData = EventSerializer.toData cryptoProvider
        DataToEvent = EventSerializer.toEvent cryptoProvider
    }
    Validators = [ ]
    TryGetRollbackEvent =
        fun state evn ->
            match evn with
            | CreditsWithdrawn args -> CreditsRefunded { Id = args.Id; Amount = args.Amount; LessonId = args.LessonId } |> Some
            | CreditsRefunded args -> CreditsWithdrawn { Id = args.Id; Amount = args.Amount; LessonId = args.LessonId } |> Some
            | CashReservationsBlocked args -> CashReservationsUnblocked { Id = args.Id } |> Some
            | CashReservationsUnblocked args ->
                if state.LastCashBlocking.IsSome then
                    CashReservationsBlocked { Id = args.Id; Expires = snd state.LastCashBlocking.Value; LessonId = fst state.LastCashBlocking.Value } |> Some
                else None
            | _ -> None
}

let get (cryptoProvider:SymetricCryptoProvider) store =
    store |> getCommandHandler (settings cryptoProvider)