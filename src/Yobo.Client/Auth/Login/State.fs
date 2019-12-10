module Yobo.Client.Auth.Login.State

open Domain
open Elmish
open Elmish.SweetAlert
open Feliz.Router
open Yobo.Shared.Auth.Validation

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | Navigate s -> model, Router.navigate(s) 
    | FormChanged f ->
        { model with Form = f; FormValidationErrors = validateLogin(f) }, Cmd.none
    | Login -> { model with IsLogging = true }, LoggedIn (async{ return Ok ""}) |> Cmd.ofMsg
    | LoggedIn res ->
        let errorAlert =
            SimpleAlert("Zadali jste nesprávný email nebo heslo")
                .Title("Přihlášení se nezdařilo")
                .Type(AlertType.Error)
                .ConfirmButton(false)
                .Timeout(3000)
        { model with IsLogging = false }, SweetAlert.Run(errorAlert) 
