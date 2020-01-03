module Yobo.Client.Pages.Registration.State

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
    | FormChanged f ->
        { model with Form = model.Form |> ValidatedForm.updateWith f |> ValidatedForm.validateConditionalWith model.FormSent validateRegister }, Cmd.none
    | Register ->
        let model = { model with FormSent = true; Form = model.Form |> ValidatedForm.validateWith validateRegister }
        if model.Form |> ValidatedForm.isValid then
            { model with IsLoading = true }, Cmd.OfAsync.eitherResult authService.Register model.Form.FormData Registered
        else model, Cmd.none
    | Registered res ->
        let onSuccess _ = { model with IsLoading = false; Form = Request.Register.init |> ValidatedForm.init }, ServerResponseViews.showSuccess "REG SUCCESS"
        let onError = { model with IsLoading = false }
        let onValidationError (m:Model) e = { m with Form = m.Form |> ValidatedForm.updateWithErrors e } 
        res |> handleValidated onSuccess onError onValidationError
    | ToggleTerms -> { model with ShowTerms = not model.ShowTerms }, Cmd.none        
        