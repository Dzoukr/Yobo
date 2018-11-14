module Yobo.Client.Login.Register.State

open Elmish
open Yobo.Client.Login.Register.Domain
open Yobo.Shared.Validation

let private updateValidation (state : State) =
    let validation = 
        if state.AlreadyTried then state.Account |> Yobo.Shared.Login.Register.Validation.validateAccount
        else state.ValidationResult
    { state with ValidationResult = validation }


let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    
    match msg with
    | Register -> 
        let validation = state.Account |> Yobo.Shared.Login.Register.Validation.validateAccount
        match validation.IsValid with
        | true -> { state with IsRegistering = true; ValidationResult = validation }, Cmd.none
        | false -> { state with AlreadyTried = true; ValidationResult = validation }, Cmd.none
    | ChangeFirstName v -> { state with Account = { state.Account with FirstName = v } } |> updateValidation, Cmd.none
    | ChangeLastName v -> { state with Account = { state.Account with LastName = v }} |> updateValidation, Cmd.none
    | ChangeEmail v -> { state with Account = { state.Account with Email = v }} |> updateValidation, Cmd.none
    | ChangePassword v -> { state with Account = { state.Account with Password = v }} |> updateValidation, Cmd.none
    | ChangeSecondPassword v -> { state with Account = { state.Account with SecondPassword = v }} |> updateValidation, Cmd.none
