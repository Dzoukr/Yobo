module Yobo.Client.Pages.Registration.View

open Yobo.Client.Router
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Yobo.Client
open Yobo.Client.Forms
open Domain
open Yobo.Client.SharedView
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
        Bulma.title.h1 "Registrace"
        
        Bulma.field.div [
            Bulma.label "Křestní jméno"
            Bulma.fieldBody [
                Bulma.input.text [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.FirstName))
                    prop.onTextChange (fun x -> { model.Form.FormData with FirstName = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.FirstName
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.FirstName))
        ]
        
        Bulma.field.div [
            Bulma.label "Příjmení"
            Bulma.fieldBody [
                Bulma.input.text [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.LastName))
                    prop.onTextChange (fun x -> { model.Form.FormData with LastName = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.LastName
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.LastName))
        ]
        
        Bulma.field.div [
            Bulma.label "Email"
            Bulma.fieldBody [
                Bulma.input.text [
                    ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.Email))
                    prop.onTextChange (fun x -> { model.Form.FormData with Email = x } |> FormChanged |> dispatch)
                    prop.valueOrDefault model.Form.FormData.Email
                ]
            ]
            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.Email))
        ]
        
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
        
        Bulma.field.p [
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
        
//        Bulma.field.div [
//            Bulma.fieldBody [
//                Html.div [
//                    Checkradio.checkbox [
//                        prop.id "newsletters"
//                        color.isSuccess
//                        prop.onCheckedChange (fun chkd -> { model.Form.FormData with NewslettersButtonChecked = chkd } |> FormChanged |> dispatch )
//                    ]
//                    Html.label [
//                        prop.htmlFor "newsletters"
//                        prop.text "Souhlasím se zasíláním informačních emailů (newsletterů)"
//                    ]
//                ]
//            ]
//            ValidationViews.help model.Form.ValidationErrors (nameof(model.Form.FormData.NewslettersButtonChecked))
//        ]
//        
        Bulma.field.div [
            Bulma.fieldBody [
                Bulma.button.button [
                    yield color.isPrimary
                    yield button.isFullWidth
                    if model.Form.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    yield prop.text "Registrovat"
                    yield prop.onClick (fun _ -> Register |> dispatch)
                ]
            ]
        ]
        Html.div [
            Html.aRouted "Zpět na přihlášení" (Anonymous Login)
        ]
    ]

let view = React.functionComponent(fun () ->
    let model, dispatch = React.useElmish(State.init, State.update, [| |])
    Html.div [
        // terms modal
        SharedView.StaticTextViews.showTermsModal model.ShowTerms (fun _ -> ToggleTerms |> dispatch)
        
        // form
        if model.ShowThankYou then
            SharedView.BoxedViews.showSuccessMessage "Registrace proběhla úspěšně! Pro aktivaci účtu klikněte na odkaz v registračním emailu."
        else            
            registerForm model dispatch            
    ]
    |> inTemplate
)