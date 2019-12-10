module Yobo.Client.Auth.State

open Domain
open Elmish

let private upTo toState toMsg (m,cmd) =
    toState(m), Cmd.map(toMsg) cmd
    
let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match model, msg with
    | Login m, LoginMsg subMsg -> Login.State.update subMsg m |> upTo Login LoginMsg
