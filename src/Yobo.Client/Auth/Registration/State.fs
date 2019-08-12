module Yobo.Client.Auth.Registration.State

open Elmish
open Yobo.Shared.Validation
open Yobo.Shared.Communication
open Yobo.Shared.Auth.Validation
open Yobo.Client.Auth.Registration.Domain
open Yobo.Client.Server

let private updateValidation (state : State) =
    let validation = 
        if state.AlreadyTried then state.Account |> validateAccount |> ValidationResult.FromResult
        else state.ValidationResult
    { state with ValidationResult = validation }


let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Register -> 
        let validation = state.Account |> validateAccount |> ValidationResult.FromResult
        match validation.IsValid with
        | true -> { state with AlreadyTried = true; IsRegistrating = true; ValidationResult = validation }, (state.Account |> Cmd.ofAsyncResult authAPI.Register Registered)
        | false -> { state with AlreadyTried = true; ValidationResult = validation }, Cmd.none
    | Registered result -> 
        match result with
        | Ok _ ->
            { state with IsRegistrating = false; RegistrationResult = Some result }, Cmd.none
        | Error (ServerError.ValidationError err) ->
            let validation = ValidationResult.FromErrorList err
            { state with IsRegistrating = false; RegistrationResult = Some result; ValidationResult = validation }, Cmd.none
        | _ -> { state with IsRegistrating = false; RegistrationResult = Some result }, Cmd.none
    | ChangeFirstName v -> { state with Account = { state.Account with FirstName = v } } |> updateValidation, Cmd.none
    | ChangeLastName v -> { state with Account = { state.Account with LastName = v }} |> updateValidation, Cmd.none
    | ChangeEmail v -> { state with Account = { state.Account with Email = v }} |> updateValidation, Cmd.none
    | ChangePassword v -> { state with Account = { state.Account with Password = v }} |> updateValidation, Cmd.none
    | ChangeSecondPassword v -> { state with Account = { state.Account with SecondPassword = v }} |> updateValidation, Cmd.none
    | ToggleAgreement -> { state with Account = { state.Account with AgreeButtonChecked = not state.Account.AgreeButtonChecked } } |> updateValidation, Cmd.none
    | ToggleTerms -> { state with ShowTerms = not state.ShowTerms }, Cmd.none
    | ToggleNewsletters -> { state with Account = { state.Account with NewslettersButtonChecked = not state.Account.NewslettersButtonChecked} }, Cmd.none
