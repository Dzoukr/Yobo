module Yobo.Client.Auth.Login.View

open Domain
open Feliz
open Feliz.Bulma
open Yobo.Client

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
                    input.isDanger
                    prop.placeholder "Váš email"
                    prop.onTextChange (fun x -> { model.Form with Email = x } |> FormChanged |> dispatch)
                ]
            ]
            Html.p [
                prop.className "help is-danger"
                prop.text "Pole nesmi byt prazdne"
            ]
        ]
        Bulma.field [
            Bulma.fieldBody [
                Bulma.passwordInput [
                    prop.placeholder "Vaše heslo"
                    prop.onTextChange (fun x -> { model.Form with Password = x } |> FormChanged |> dispatch)
                ]
            ]
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
            Html.a [ prop.text "Registrace"; prop.onClick (fun _ -> Navigate(Paths.Registration) |> dispatch) ]
            Html.span " · "
            Html.a [ prop.text "Zapomněl(a) jsem heslo!"; prop.onClick (fun _ -> Navigate(Paths.ForgottenPassword) |> dispatch)  ]
        ]
    ]
    |> inTemplate