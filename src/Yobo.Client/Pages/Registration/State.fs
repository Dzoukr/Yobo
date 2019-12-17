module Yobo.Client.Pages.Registration.State

open Domain
open Elmish
open Yobo.Shared.Auth.Validation
open Yobo.Client.Server
open Yobo.Client.SharedView
open Yobo.Client.StateHandlers
open Yobo.Shared.Auth.Communication

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | FormChanged f ->
        let validation = if model.FormSent then validateLogin(f) else []
        { model with Form = f; FormValidationErrors = validation }, Cmd.none
    | Login ->
        let validationErrors = validateLogin(model.Form)
        let model = { model with FormSent = true; FormValidationErrors = validationErrors }
        match validationErrors with
        | [] -> { model with IsLogging = true }, Cmd.OfAsync.eitherResult authService.GetToken model.Form LoggedIn
        | _ -> model, Cmd.none
    | LoggedIn res ->
        let onSuccess token = { model with IsLogging = false; Form = Request.Login.init }, ServerResponseViews.showSuccess "Byli jste úspěšně přihlášeni!"
        let onError = { model with IsLogging = false }
        let onValidationError m e = { m with FormValidationErrors = e } 
        res |> handleValidated onSuccess onError onValidationError
        