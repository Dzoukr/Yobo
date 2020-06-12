module Yobo.Client.Pages.AccountActivation.State

open Domain
open Elmish
open Yobo.Shared.Auth.Validation
open Yobo.Client.Server
open Yobo.Client.SharedView
open Yobo.Client.StateHandlers
open Yobo.Shared.Auth.Communication
open Yobo.Client.Forms

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | Activate -> model, Cmd.OfAsync.eitherAsResult authService.ActivateAccount model.ActivationId Activated
    | Activated res -> { model with ActivationResult = Some res }, Cmd.none
        