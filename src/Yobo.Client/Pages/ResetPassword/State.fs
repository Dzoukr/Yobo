module Yobo.Client.Pages.ResetPassword.State

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
        { model with Form = model.Form |> ValidatedForm.updateWith f |> ValidatedForm.validateConditionalWith model.FormSent validateResetPassword }, Cmd.none
    | Reset ->
        let model = { model with FormSent = true; Form = model.Form |> ValidatedForm.validateWith validateResetPassword }
        if model.Form |> ValidatedForm.isValid then
            { model with IsLoading = true }, Cmd.OfAsync.eitherAsResult authService.ResetPassword model.Form.FormData Resetted
        else model, Cmd.none
    | Resetted res ->
        let onSuccess _ =
            { model with IsLoading = false; Form = Request.ResetPassword.init |> ValidatedForm.init },
            [
                ServerResponseViews.showSuccessToast "Vaše heslo bylo úspěšně nastaveno. Nyní se můžete přihlásit."
                (Router.navigatePath Paths.Login)
            ] |> Cmd.batch
        let onError = { model with IsLoading = false }
        let onValidationError (m:Model) e = { m with Form = m.Form |> ValidatedForm.updateWithErrors e } 
        res |> handleValidated onSuccess onError onValidationError    
        