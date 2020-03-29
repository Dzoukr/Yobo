module Yobo.Client.Pages.ForgottenPassword.State

open Domain
open Elmish
open Feliz.Router
open Yobo.Client.Router
open Yobo.Client
open Yobo.Shared.Auth.Validation
open Yobo.Client.Server
open Yobo.Client.SharedView
open Yobo.Client.StateHandlers
open Yobo.Shared.Auth.Communication
open Yobo.Client.Forms

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | FormChanged f ->
        { model with Form = model.Form |> ValidatedForm.updateWith f |> ValidatedForm.validateWithIfSent validateForgottenPassword }, Cmd.none
    | InitiateReset ->
        let model = { model with Form = model.Form |> ValidatedForm.validateWith validateForgottenPassword |> ValidatedForm.markAsSent }
        if model.Form |> ValidatedForm.isValid then
            { model with Form = model.Form |> ValidatedForm.startLoading }, Cmd.OfAsync.eitherAsResult authService.ForgottenPassword model.Form.FormData ResetInitiated
        else model, Cmd.none
    | ResetInitiated res ->
        let onSuccess _ =
            { model with Form = Request.ForgottenPassword.init |> ValidatedForm.init |> ValidatedForm.stopLoading },
            [
                ServerResponseViews.showSuccessToast "Pokud jste zadali váš email správně, brzy dorazí do vaší emailové schránky další instrukce."
                (Router.navigatePage (Anonymous Login))
            ] |> Cmd.batch
        let onError = { model with Form = model.Form |> ValidatedForm.stopLoading }
        let onValidationError (m:Model) e = { m with Form = m.Form |> ValidatedForm.updateWithErrors e } 
        res |> handleValidated onSuccess onError onValidationError        