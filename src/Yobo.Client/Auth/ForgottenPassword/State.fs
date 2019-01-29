module Yobo.Client.Auth.ForgottenPassword.State

open Elmish
open Yobo.Client.Http
open Domain

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | EmailChanged v -> { state with EmailToReset = v }, Cmd.none
    | InitiateReset -> state, (state.EmailToReset |> Cmd.ofAsyncResult authAPI.InitiatePasswordReset ResetInitiated)
    | ResetInitiated res -> { state with ResetResult = Some res }, Cmd.none