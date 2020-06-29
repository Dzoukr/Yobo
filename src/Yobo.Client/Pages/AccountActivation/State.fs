module Yobo.Client.Pages.AccountActivation.State

open Domain
open Elmish
open Yobo.Client.Server
open Yobo.Shared.Auth.Communication

let init id  =
    {
        ActivationId = id
        ActivationResult = None
    }, Cmd.ofMsg Activate

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | Activate -> model, Cmd.OfAsync.eitherAsResult authService.ActivateAccount model.ActivationId Activated
    | Activated res -> { model with ActivationResult = Some res }, Cmd.none
        