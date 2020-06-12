module Yobo.Client.Pages.ForgottenPassword.View

open Feliz
open Feliz.Bulma
open Yobo.Client.Router
open Yobo.Client.Forms
open Domain
open Yobo.Client.SharedView

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
        Bulma.title.h1 "Zapomenuté heslo"
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
                Bulma.button.button [
                    yield color.isPrimary
                    yield button.isFullWidth
                    if model.Form.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    yield prop.text "Resetovat heslo"
                    yield prop.onClick (fun _ -> InitiateReset |> dispatch)
                ]
            ]
        ]
        
        Html.div [
            Html.aRouted "Zpátky na přihlášení" (Anonymous Login)
        ]
    ]
    |> inTemplate