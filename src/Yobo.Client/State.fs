module Yobo.Client.State

open Domain
open Elmish

let init () = Model.init, Cmd.none

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | UrlChanged p -> { currentModel with CurrentPage = p }, Cmd.none
    | ToggleQuickView ->  { currentModel with ShowQuickView = not currentModel.ShowQuickView }, Cmd.none