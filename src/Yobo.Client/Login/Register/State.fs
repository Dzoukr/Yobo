module Yobo.Client.Login.Register.State

open Elmish
open Yobo.Client.Login.Register.Domain

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Register -> { state with IsRegistering = true }, Cmd.none
    | ChangeFirstName v -> { state with Account = { state.Account with FirstName = v }}, Cmd.none
    | ChangeLastName v -> { state with Account = { state.Account with LastName = v }}, Cmd.none
    | ChangeEmail v -> { state with Account = { state.Account with Email = v }}, Cmd.none
    | ChangePassword v -> { state with Account = { state.Account with Password = v }}, Cmd.none
    | ChangeSecondPassword v -> { state with Account = { state.Account with SecondPassword = v }}, Cmd.none
