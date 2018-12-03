module Yobo.Client.Login.State

open Yobo.Client.Login.Domain
open Elmish

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Login -> { state with IsLogging = true }, (Http.login state.Login)
    | LoginDone res -> { state with IsLogging = false; LoginResult = Some res }, Cmd.none
    | ChangeEmail v -> { state with Login = { state.Login with Email = v }}, Cmd.none
    | ChangePassword v -> { state with Login = { state.Login with Password = v }}, Cmd.none