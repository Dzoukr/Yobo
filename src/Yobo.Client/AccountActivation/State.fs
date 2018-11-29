module Yobo.Client.AccountActivation.State

open Yobo.Client.AccountActivation.Domain
open Elmish

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Activate id -> state, Http.activate id
    | ActivateDone res -> { state with ActivationResult = Some res }, Cmd.none
