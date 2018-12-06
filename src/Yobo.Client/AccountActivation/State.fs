module Yobo.Client.AccountActivation.State

open Yobo.Client.AccountActivation.Domain
open Elmish
open Yobo.Client.Http

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Activate id -> state, (id |> Cmd.ofAsyncResult registrationAPI.ActivateAccount ActivateDone)
    | ActivateDone res -> { state with ActivationResult = Some res }, Cmd.none
