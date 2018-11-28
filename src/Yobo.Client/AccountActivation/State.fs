module Yobo.Client.AccountActivation.State

open Yobo.Client.AccountActivation.Domain
open Elmish

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Activate id -> { state with IsActivating = true }, Http.activate id
    | ActivateDone res -> { state with IsActivating = false; ActivationResult = Some res }, Cmd.none
