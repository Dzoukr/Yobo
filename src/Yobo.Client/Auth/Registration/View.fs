module Yobo.Client.Auth.Registration.View

open System
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Core.JsInterop
open Yobo.Client.Auth.Registration.Domain
open Yobo.Shared
open Yobo.Client

let render (state : State) (dispatch : Msg -> unit) =
    let regInput value typ msgType txt =
        let error = state.ValidationResult |> Validation.tryGetFieldError txt
        let clr = if error.IsSome then Input.Color IsDanger else Input.Option.Props []
        let help = if error.IsSome then 
                    Help.help [ Help.Color IsDanger ]
                        [ str (error.Value.Explain()) ]
                   else span [] []
        Control.div [] [
            typ [
                clr
                Input.Option.Value value
                Input.Option.OnChange (fun e -> !!e.target?value |> msgType |> dispatch)
            ]
            help
        ]

    let lbl txt = Label.label [] [ str txt ]

    let btn isInProgress =
        let content = if isInProgress then i [ ClassName "fa fa-circle-o-notch fa-spin" ] [] else str "Registrovat"
        Control.div [] [
            Button.button 
                [ Button.Color IsPrimary; Button.IsFullWidth; Button.OnClick (fun _ -> dispatch Msg.Register); Button.Disabled(isInProgress) ]
                [ content ]
        ]
    
    let form = 
        div 
            [ ClassName "box"] 
            [
                Heading.h1 [ ] [ str "Registrace" ]

                (SharedView.serverErrorToViewIfAny state.RegistrationResult)

                Field.div [] [
                    lbl "Křestní jméno"
                    regInput state.Account.FirstName Input.text ChangeFirstName "FirstName"
                ]

                Field.div [] [
                    lbl "Příjmení"
                    regInput state.Account.LastName Input.text ChangeLastName "LastName"
                ]

                Field.div [] [
                    lbl "Email"
                    regInput state.Account.Email Input.email ChangeEmail "Email"
                ]

                Field.div [] [
                    lbl "Heslo"
                    regInput state.Account.Password Input.password ChangePassword "Password"
                ]

                Field.div [] [
                    lbl "Heslo (ještě jednou pro kontrolu)"
                    regInput state.Account.SecondPassword Input.password ChangeSecondPassword "SecondPassword"

                ]

                Field.div [] [
                    btn state.IsRegistrating
                ]
                a [ Href Router.Routes.login; OnClick Router.goToUrl] [
                    str "Zpět na přihlášení"
                ]   
            ]

    let content = 
        match state.RegistrationResult with
        | None | Some (Error _) -> form
        | Some (Ok _) ->
            "Registrace proběhla úspěšně. Nyní je potřeba zaktivovat váš účet. Podívejte se prosím do emailu, kam by vám měl přijít aktivační odkaz a klikněte na něj."
            |> str
            |> SharedView.successBox

    Hero.hero [ ]
        [ Hero.body [ ]
            [ Container.container 
                [ Container.IsFluid; Container.Props [ ClassName "has-text-centered"] ]
                [ Column.column 
                    [ Column.Width (Screen.All, Column.Is4); Column.Offset (Screen.All, Column.Is4) ] 
                    [ content ] 
                ] 
            ]
        ]