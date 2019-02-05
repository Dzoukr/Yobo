module Yobo.Client.Auth.ResetPassword.State

open Elmish
open Yobo.Client.Http
open Domain
open Yobo.Shared.Auth
open Yobo.Shared.Validation

let private updateValidation (state : State) =
    let validation =
        if state.AlreadyTried then state.Form |> Validation.validatePasswordReset |> ValidationResult.FromResult
        else state.ValidationResult
    { state with ValidationResult = validation }

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | FormChanged f -> { state with Form = f } |> updateValidation, Cmd.none
    | ResetPassword ->
        let validation = state.Form |> Validation.validatePasswordReset |> ValidationResult.FromResult
        match validation.IsValid with
        | true ->
            { state with AlreadyTried = true }, ((state.PasswordResetKey, state.Form) |> Cmd.ofAsyncResult authAPI.ResetPassword PasswordReset)
        | false -> { state with AlreadyTried = true; ValidationResult = validation }, Cmd.none
    | PasswordReset res -> { state with ResetResult = Some res }, Cmd.none
