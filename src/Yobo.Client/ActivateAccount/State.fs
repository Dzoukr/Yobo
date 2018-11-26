module Yobo.Client.ActivateAccount.State

open Elmish
open Yobo.Client.ActivateAccount.Domain

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    state, Cmd.none
    //match msg with
    //| Login -> { state with IsLogging = true }, Cmd.none
    //| ChangeEmail v -> { state with Credentials = { state.Credentials with Email = v }}, Cmd.none
    //| ChangePassword v -> { state with Credentials = { state.Credentials with Password = v }}, Cmd.none
