﻿module Yobo.Client.Pages.Registration.View

open Feliz
open Feliz.Bulma
open Yobo.Client
open Yobo.Client.Forms
open Domain
open Yobo.Client.SharedView
open Feliz.Router
open Feliz.Bulma.Checkradio

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

let registerForm model dispatch =
    Bulma.box [
        Bulma.title1 "Registrace"
        
        Bulma.field [
            Bulma.label "Křestní jméno"
            Bulma.fieldBody [
                Bulma.textInput [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.FirstName))
                    prop.onTextChange (fun x -> { model.Form.FormData with FirstName = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.FirstName
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.FirstName))
        ]
        
        Bulma.field [
            Bulma.label "Příjmení"
            Bulma.fieldBody [
                Bulma.textInput [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.LastName))
                    prop.onTextChange (fun x -> { model.Form.FormData with LastName = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.LastName
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.LastName))
        ]
        
        Bulma.field [
            Bulma.label "Email"
            Bulma.fieldBody [
                Bulma.textInput [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.Email))
                    prop.onTextChange (fun x -> { model.Form.FormData with Email = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.Email
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.Email))
        ]
        
        Bulma.field [
            Bulma.label "Heslo"
            Bulma.fieldBody [
                Bulma.passwordInput [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.Password))
                    prop.onTextChange (fun x -> { model.Form.FormData with Password = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.Password
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.Password))
        ]
        
        Bulma.field [
            Bulma.label "Heslo (ještě jednou pro kontrolu)"
            Bulma.fieldBody [
                Bulma.passwordInput [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.SecondPassword))
                    prop.onTextChange (fun x -> { model.Form.FormData with SecondPassword = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.SecondPassword
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.SecondPassword))
        ]
        
        Bulma.field [
            Bulma.fieldBody [
                Checkradio.checkbox [
                    prop.id "terms"
                    color.isSuccess
                    prop.isChecked model.Form.FormData.AgreeButtonChecked
                    prop.onCheckedChange (fun chkd -> { model.Form.FormData with AgreeButtonChecked = chkd } |> FormChanged |> dispatch )
                ]
                Html.label [
                    prop.htmlFor "terms"
                    prop.text "Souhlasím s obchodními podmínkami"
                ]
            ]
            Html.div [
                Html.a [ prop.onClick (fun _ -> ToggleTerms |> dispatch); prop.text "Obchodní podmínky" ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.AgreeButtonChecked))
        ]
        
        Bulma.field [
            Bulma.fieldBody [
                Checkradio.checkbox [
                    prop.id "newsletters"
                    color.isSuccess
                    prop.onCheckedChange (fun chkd -> { model.Form.FormData with NewslettersButtonChecked = chkd } |> FormChanged |> dispatch )
                ]
                Html.label [
                    prop.htmlFor "newsletters"
                    prop.text "Souhlasím se zasíláním informačních emailů (newsletterů)"
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.NewslettersButtonChecked))
        ]
        
        Bulma.field [
            Bulma.fieldBody [
                Bulma.button [
                    yield button.isPrimary
                    yield button.isFullwidth
                    if model.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    yield prop.text "Registrovat"
                    yield prop.onClick (fun _ -> Register |> dispatch)
                ]
            ]
        ]
        Html.div [
            Html.a [ prop.text "Zpět na přihlášení"; prop.href (Router.format Paths.Login);  prop.onClick Router.goToUrl  ]
        ]
    ]


let view (model:Model) (dispatch:Msg -> unit) =
    Html.div [
        // terms modal
        SharedView.StaticTextViews.termsModal model.ShowTerms (fun _ -> ToggleTerms |> dispatch)
        
        // form
        if model.ShowThankYou then
            SharedView.BoxedViews.showSuccess "Registrace proběhla úspěšně! Pro aktivaci účtu klikněte na odkaz v registračním emailu."
        else            
            registerForm model dispatch            
    ]
    |> inTemplate