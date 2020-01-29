module Yobo.Client.Pages.ForgottenPassword.State

open Domain
open Elmish
open Feliz.Router
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
        { model with Form = model.Form |> ValidatedForm.updateWith f |> ValidatedForm.validateConditionalWith model.FormSent validateForgottenPassword }, Cmd.none
    | InitiateReset ->
        let model = { model with FormSent = true; Form = model.Form |> ValidatedForm.validateWith validateForgottenPassword }
        if model.Form |> ValidatedForm.isValid then
            { model with IsLoading = true }, Cmd.OfAsync.eitherResult authService.ForgottenPassword model.Form.FormData ResetInitiated
        else model, Cmd.none
    | ResetInitiated res ->
        let onSuccess _ =
            { model with IsLoading = false; Form = Request.ForgottenPassword.init |> ValidatedForm.init },
            [
                ServerResponseViews.showSuccessToast "Pokud jste zadali váš email správně, brzy dorazí do vaší emailové schránky další instrukce."
                (Router.navigate Paths.Login)
            ] |> Cmd.batch
        let onError = { model with IsLoading = false }
        let onValidationError (m:Model) e = { m with Form = m.Form |> ValidatedForm.updateWithErrors e } 
        res |> handleValidated onSuccess onError onValidationError        