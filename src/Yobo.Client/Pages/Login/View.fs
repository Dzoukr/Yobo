﻿module Yobo.Client.Pages.Login.View

open Yobo.Client.Router
open Feliz
open Feliz.Bulma
open Yobo.Client
open Yobo.Client.Forms
open Domain
open Yobo.Client.SharedView
open Feliz.Router

let inTemplate (content:ReactElement) =
    Bulma.hero [
        Bulma.heroBody [
            Bulma.columns [
                Bulma.column [
                    column.is4
                    column.isOffset4
                    text.hasTextCentered
                    prop.children content
                ]
            ]
        ]
    ]

let view (model:Model) (dispatch:Msg -> unit) =
    Bulma.box [
        Html.img [ prop.src "img/logo.png" ]
        Bulma.field.div [
            Bulma.fieldBody [
                Bulma.input.text [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.Email))
                    prop.placeholder "Váš email"
                    prop.onTextChange (fun x -> { model.Form.FormData with Email = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.Email
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.Email))
        ]
        Bulma.field.div [
            Bulma.fieldBody [
                Bulma.input.password [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.Password))
                    prop.placeholder "Vaše heslo"
                    prop.onTextChange (fun x -> { model.Form.FormData with Password = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault (model.Form.FormData.Password)
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.Password))
        ]
        Bulma.field.div [
            Bulma.fieldBody [
                Bulma.button.button [
                    yield color.isPrimary
                    yield button.isFullWidth
                    if model.Form.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    yield prop.text "Přihlásit se"
                    yield prop.onClick (fun _ -> Login |> dispatch)
                ]
            ]
        ]
        Html.div [
            Html.aRouted "Registrace" (Anonymous Registration)
            Html.span " · "
            Html.aRouted "Zapomněl(a) jsem heslo!" (Anonymous ForgottenPassword)
        ]
    ]
    |> inTemplate