module Yobo.Client.Auth.Login.State

open Domain
open Elmish
open Elmish.SweetAlert
open Yobo.Shared.Auth.Validation
open Elmish.Toastr
open Yobo.Client
open Yobo.Client.Server

//let successToast : Cmd<Msg> = 
//    Toastr.message "Success message"
//    |> Toastr.title "Shiny title"
//    |> Toastr.position ToastPosition.BottomRight
//    |> Toastr.timeout 3000
//    |> Toastr.withProgressBar
//    |> Toastr.hideEasing Easing.Swing
//    |> Toastr.showCloseButton
//    |> Toastr.success

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | FormChanged f ->
        let validation = if model.FormSent then validateLogin(f) else []
        { model with Form = f; FormValidationErrors = validation }, Cmd.none
    | Login ->
        let validationErrors = validateLogin(model.Form)
        let model = { model with FormSent = true; FormValidationErrors = validationErrors }
        match validationErrors with
        | [] -> { model with IsLogging = true }, Cmd.OfAsync.eitherResult authService.Login model.Form LoggedIn
        | _ -> model, Cmd.none
    | LoggedIn res ->
        
        
        let errorAlert =
            SimpleAlert("Zadali jste nesprávný email nebo heslo")
                .Title("Přihlášení se nezdařilo")
                .Type(AlertType.Error)
                .ConfirmButton(false)
                .Timeout(3000)
        { model with IsLogging = false }, SweetAlert.Run(errorAlert) 
