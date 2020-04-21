module Yobo.Client.Pages.ResetPassword.View

open Domain
open Yobo.Client.Router
open Feliz
open Feliz.Router
open Feliz.Bulma
open Yobo.Client.SharedView
open Yobo.Shared.Errors

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
        Bulma.title.h1 "Nastavení nového hesla"
        Bulma.field.div [
            Bulma.label "Heslo"
            Bulma.fieldBody [
                Bulma.input.password [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.Password))
                    prop.onTextChange (fun x -> { model.Form.FormData with Password = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.Password
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.Password))
        ]
        
        Bulma.field.div [
            Bulma.label "Heslo (ještě jednou pro kontrolu)"
            Bulma.fieldBody [
                Bulma.input.password [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.SecondPassword))
                    prop.onTextChange (fun x -> { model.Form.FormData with SecondPassword = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.SecondPassword
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.SecondPassword))
        ]
        Bulma.field.div [
            Bulma.fieldBody [
                Bulma.button.button [
                    yield color.isPrimary
                    yield button.isFullWidth
                    if model.Form.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    yield prop.text "Nastavit heslo"
                    yield prop.onClick (fun _ -> Reset |> dispatch)
                ]
            ]
        ]
        
        Html.div [
            Html.aRouted "Zpátky na přihlášení" (Anonymous Login)
        ]
    ]
    |> inTemplate