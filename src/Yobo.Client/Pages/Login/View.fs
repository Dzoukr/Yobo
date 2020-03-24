module Yobo.Client.Pages.Login.View

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
        Bulma.field [
            Bulma.fieldBody [
                Bulma.textInput [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.Email))
                    prop.placeholder "Váš email"
                    prop.onTextChange (fun x -> { model.Form.FormData with Email = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.Email
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.Email))
        ]
        Bulma.field [
            Bulma.fieldBody [
                Bulma.passwordInput [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.Password))
                    prop.placeholder "Vaše heslo"
                    prop.onTextChange (fun x -> { model.Form.FormData with Password = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault (model.Form.FormData.Password)
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.Password))
        ]
        Bulma.field [
            Bulma.fieldBody [
                Bulma.button [
                    yield button.isPrimary
                    yield button.isFullwidth
                    if model.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    yield prop.text "Přihlásit se"
                    yield prop.onClick (fun _ -> Login |> dispatch)
                ]
            ]
        ]
        Html.div [
            Html.a [ prop.text "Registrace"; prop.href (Router.formatPath Paths.Registration); prop.onClick Router.goToUrl ]
            Html.span " · "
            Html.a [ prop.text "Zapomněl(a) jsem heslo!"; prop.href (Router.formatPath Paths.ForgottenPassword); prop.onClick Router.goToUrl  ]
        ]
    ]
    |> inTemplate