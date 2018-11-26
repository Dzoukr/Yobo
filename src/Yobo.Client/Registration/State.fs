module Yobo.Client.Registration.State

open Elmish
open Yobo.Shared.Validation
open Yobo.Shared.Communication
open Yobo.Shared.Registration.Validation
open Yobo.Client.Registration.Domain

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
        | true -> { state with AlreadyTried = true; IsRegistrationing = true; ValidationResult = validation }, Http.registration(state.Account)
        | false -> { state with AlreadyTried = true; ValidationResult = validation }, Cmd.none
    | RegisterDone result -> 
        match result with
        | Ok _ ->
            { state with IsRegistrationing = false; RegistrationResult = Some result }, Cmd.none
        | Error (ServerError.ValidationError err) ->
            let validation = ValidationResult.FromErrorList err
            { state with IsRegistrationing = false; RegistrationResult = Some result; ValidationResult = validation }, Cmd.none
        | _ -> { state with IsRegistrationing = false; RegistrationResult = Some result }, Cmd.none
    | ChangeFirstName v -> { state with Account = { state.Account with FirstName = v } } |> updateValidation, Cmd.none
    | ChangeLastName v -> { state with Account = { state.Account with LastName = v }} |> updateValidation, Cmd.none
    | ChangeEmail v -> { state with Account = { state.Account with Email = v }} |> updateValidation, Cmd.none
    | ChangePassword v -> { state with Account = { state.Account with Password = v }} |> updateValidation, Cmd.none
    | ChangeSecondPassword v -> { state with Account = { state.Account with SecondPassword = v }} |> updateValidation, Cmd.none
