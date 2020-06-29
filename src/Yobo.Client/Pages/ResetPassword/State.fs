module Yobo.Client.Pages.ResetPassword.State

open Domain
open Elmish
open Yobo.Client.Router
open Yobo.Client
open Yobo.Shared.Auth.Validation
open Yobo.Client.Server
open Yobo.Client.SharedView
open Yobo.Client.StateHandlers
open Yobo.Shared.Auth.Communication
open Yobo.Client.Forms

let init key  =
    {
        Form = Request.ResetPassword.init |> fun x -> { x with PasswordResetKey = key } |> ValidatedForm.init
    }, Cmd.none

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | FormChanged f ->
        { model with Form = model.Form |> ValidatedForm.updateWith f |> ValidatedForm.validateWithIfSent validateResetPassword }, Cmd.none
    | Reset ->
        let model = { model with Form = model.Form |> ValidatedForm.validateWith validateResetPassword |> ValidatedForm.markAsSent }
        if model.Form |> ValidatedForm.isValid then
            { model with Form = model.Form |> ValidatedForm.startLoading }, Cmd.OfAsync.eitherAsResult authService.ResetPassword model.Form.FormData Resetted
        else model, Cmd.none
    | Resetted res ->
        let onSuccess _ =
            { model with Form = Request.ResetPassword.init |> ValidatedForm.init |> ValidatedForm.stopLoading },
            [
                ServerResponseViews.showSuccessToast "Vaše heslo bylo úspěšně nastaveno. Nyní se můžete přihlásit."
                (Router.navigatePage (Anonymous Login))
            ] |> Cmd.batch
        let onError = { model with Form = model.Form |> ValidatedForm.stopLoading }
        let onValidationError (m:Model) e = { m with Form = m.Form |> ValidatedForm.updateWithErrors e } 
        res |> handleValidated onSuccess onError onValidationError    
        