module Yobo.Core.Users.Aggregate

open FSharp.Rop
open Yobo.Shared.Text
open Yobo.Shared.Domain
open Yobo.Core.Users

let onlyIfDoesNotExist state =
    if state.Id = State.Init.Id then Ok state
    else DomainError.ItemAlreadyExists(TextValue.Email) |> Error

let onlyIfExists state =
    if state.Id = State.Init.Id then DomainError.ItemDoesNotExist(TextValue.Email) |> Error else Ok state

let onlyIfActivationKeyMatch key state =
    if state.ActivationKey = key then Ok state else DomainError.ActivationKeyDoesNotMatch |> Error

let onlyIfNotAlreadyActivated state =
    if state.IsActivated then DomainError.UserAlreadyActivated |> Error else Ok state

let onlyIfActivated state =
    if state.IsActivated then Ok state else DomainError.UserNotActivated |> Error

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
    | AddCredits args ->
        onlyIfActivated state
        <!> (fun _ -> CreditsAdded args)
        <!> List.singleton

let apply (state:State) = function
    | Registered args -> { state with Id = args.Id; ActivationKey = args.ActivationKey }
    | ActivationKeyRegenerated args -> { state with ActivationKey = args.ActivationKey }
    | Activated args -> { state with Id = args.Id; IsActivated = true }
    | CreditsAdded args -> { state with Credits = state.Credits + args.Credits; CreditsExpirationUtc = Some args.ExpirationUtc }