module Yobo.Core.Users.Aggregate

open FSharp.Rop
open Yobo.Shared.Domain
open Yobo.Core.Users
open System

let onlyIfDoesNotExist state =
    if state.Id = State.Init.Id then Ok state
    else DomainError.ItemAlreadyExists "Email" |> Error

let onlyIfExists state =
    if state.Id = State.Init.Id then DomainError.ItemDoesNotExist "Email" |> Error else Ok state

let onlyIfActivationKeyMatch key state =
    if state.ActivationKey = key then Ok state else DomainError.ActivationKeyDoesNotMatch |> Error

let onlyIfNotAlreadyActivated state =
    if state.IsActivated then DomainError.UserAlreadyActivated |> Error else Ok state

let onlyIfActivated state =
    if state.IsActivated then Ok state else DomainError.UserNotActivated |> Error

let onlyIfEnoughCredits amount state =
    if state.Credits - amount < 0 then DomainError.NotEnoughCredits |> Error else Ok state

let onlyIfNotAlreadyBlocked state =
    match state.CashReservationsBlockedUntil with
    | None -> Ok state
    | Some d ->
        if d < DateTimeOffset.Now then Ok state else DomainError.CashReservationIsBlocked |> Error


let normalizeCreate (args:CmdArgs.Register) = { args with Email = args.Email.ToLower() }

let execute (state:State) = function
    | Register args ->
        onlyIfDoesNotExist state
        <!> (fun _ -> normalizeCreate args)
        <!> (fun a -> Registered a)
        <!> List.singleton
    | RegenerateActivationKey args ->
        onlyIfNotAlreadyActivated state
        >>= onlyIfExists
        <!> (fun _ -> ActivationKeyRegenerated args)
        <!> List.singleton
    | Activate args ->
        onlyIfNotAlreadyActivated state
        >>= onlyIfExists
        >>= onlyIfActivationKeyMatch args.ActivationKey
        <!> (fun _ -> Activated args)
        <!> List.singleton
    | InitiatePasswordReset args ->
        onlyIfExists state
        >>= onlyIfActivated
        <!> (fun _ -> PasswordResetInitiated args)
        <!> List.singleton
    | AddCredits args ->
        onlyIfActivated state
        <!> (fun _ -> CreditsAdded args)
        <!> List.singleton
    | WithdrawCredits args ->
        onlyIfActivated state
        >>= onlyIfEnoughCredits args.Amount
        <!> (fun _ -> CreditsWithdrawn args)
        <!> List.singleton
    | RefundCredits args ->
        onlyIfActivated state
        <!> (fun _ -> CreditsRefunded args)
        <!> List.singleton
    | BlockCashReservations args ->
        onlyIfActivated state
        >>= onlyIfNotAlreadyBlocked
        <!> (fun _ -> CashReservationsBlocked args)
        <!> List.singleton
    | UnblockCashReservations args ->
        onlyIfActivated state
        <!> (fun _ -> CashReservationsUnblocked args)
        <!> List.singleton

let apply (state:State) = function
    | Registered args -> { state with Id = args.Id; ActivationKey = args.ActivationKey }
    | ActivationKeyRegenerated args -> { state with ActivationKey = args.ActivationKey }
    | Activated _ -> { state with IsActivated = true }
    | PasswordResetInitiated args -> { state with PasswordResetKey = Some args.PasswordResetKey }
    | CreditsAdded args -> { state with Credits = state.Credits + args.Credits; CreditsExpiration = Some args.Expiration }
    | CreditsWithdrawn args -> { state with Credits = state.Credits - args.Amount }
    | CreditsRefunded args -> { state with Credits = state.Credits + args.Amount }
    | CashReservationsBlocked args -> { state with CashReservationsBlockedUntil = Some args.Expires; LastCashBlocking = Some (args.LessonId, args.Expires) }
    | CashReservationsUnblocked _ -> { state with CashReservationsBlockedUntil = None }