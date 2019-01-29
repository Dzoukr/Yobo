module Yobo.Client.Auth.ResetPassword.State

open Elmish
open Yobo.Client.Http
open Domain

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | FormChanged f -> { state with Form = f }, Cmd.none
    | ResetPassword -> state, Cmd.none
    //| InitiateReset -> state, (state.EmailToReset |> Cmd.ofAsyncResult authAPI.InitiatePasswordReset ResetInitiated)
    //| ResetInitiated res -> { state with ResetResult = Some res }, Cmd.none