module Yobo.Client.Pages.Registration.View

open Domain
open Feliz
open Feliz.Bulma
open Yobo.Client
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
        Bulma.title "REGI"
        Html.img [ prop.src "img/logo.png" ]
        Bulma.field [
            Bulma.fieldBody [
                Bulma.textInput [
                    ValidationViews.color model.FormValidationErrors (nameof(model.Form.Email))
                    prop.placeholder "Váš email"
                    prop.onTextChange (fun x -> { model.Form with Email = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.Email
                ]
            ]
            ValidationViews.help model.FormValidationErrors (nameof(model.Form.Email))
        ]
        Bulma.field [
            Bulma.fieldBody [
                Bulma.passwordInput [
                    ValidationViews.color model.FormValidationErrors (nameof(model.Form.Password))
                    prop.placeholder "Vaše heslo"
                    prop.onTextChange (fun x -> { model.Form with Password = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.Password
                ]
            ]
            ValidationViews.help model.FormValidationErrors (nameof(model.Form.Password))
        ]
        Bulma.field [
            Bulma.fieldBody [
                Bulma.button [
                    yield button.isPrimary
                    yield button.isFullwidth
                    if model.IsLogging then yield! [ button.isLoading; prop.disabled true ]
                    yield prop.text "Přihlásit se"
                    yield prop.onClick (fun _ -> Login |> dispatch)
                ]
            ]
        ]
        Html.div [
            Html.a [ prop.text "Registrace"; prop.href (Router.format Paths.Registration);  prop.onClick Router.goToUrl ]
            Html.span " · "
            Html.a [ prop.text "Zapomněl(a) jsem heslo!"; prop.href (Router.format Paths.ForgottenPassword);  prop.onClick Router.goToUrl  ]
        ]
    ]
    |> inTemplate