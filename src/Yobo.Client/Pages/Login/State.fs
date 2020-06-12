module Yobo.Client.Pages.Login.State

open Yobo.Client.Router
open Domain
open Elmish
open Fable.Core
open Feliz.Router
open Yobo.Client
open Yobo.Shared.Auth.Validation
open Yobo.Client.Server
open Yobo.Client.SharedView
open Yobo.Shared.Auth.Communication
open Yobo.Client.Forms

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | FormChanged f ->
        { model with Form = model.Form |> ValidatedForm.updateWith f |> ValidatedForm.validateWithIfSent validateLogin }, Cmd.none
    | Login ->
        let model = { model with Form = model.Form |> ValidatedForm.validateWith validateLogin |> ValidatedForm.markAsSent }
        if model.Form |> ValidatedForm.isValid then
            { model with Form = model.Form |> ValidatedForm.startLoading }, Cmd.OfAsync.eitherAsResult authService.GetToken model.Form.FormData LoggedIn
        else model, Cmd.none
    | LoggedIn res ->
        let model = { model with Form = model.Form |> ValidatedForm.stopLoading }
        match res with
        | Ok token ->
            TokenStorage.setToken token
            { model with Form = Request.Login.init |> ValidatedForm.init },
                Cmd.batch [ ServerResponseViews.showSuccessToast "Byli jste úspěšně přihlášeni!"; Router.navigatePage Page.defaultPage ]
        | Error e ->
            model, e |> ServerResponseViews.showErrorToast