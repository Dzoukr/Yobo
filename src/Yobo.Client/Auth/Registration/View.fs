module Yobo.Client.Auth.Registration.View

open System
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Core.JsInterop
open Yobo.Client.Auth.Registration.Domain
open Yobo.Shared
open Yobo.Shared.Text
open Yobo.Client

let render (state : State) (dispatch : Msg -> unit) =
    let regInput value typ msgType txt =
        let error = state.ValidationResult |> Validation.tryGetFieldError txt
        let clr = if error.IsSome then Input.Color IsDanger else Input.Option.Props []
        let help = if error.IsSome then 
                    Help.help [ Help.Color IsDanger ]
                        [ str (error.Value |> Locale.validationErrorToCz ) ]
                   else span [] []
        Control.div [] [
            typ [
                clr
                Input.Option.Value value
                Input.Option.OnChange (fun e -> !!e.target?value |> msgType |> dispatch)
            ]
            help
        ]

    let lbl txt = Label.label [] [ str (Locale.toTitleCz txt) ]

    let btn isInProgress =
        let content = if isInProgress then i [ ClassName "fa fa-circle-o-notch fa-spin" ] [] else str (Locale.toTitleCz Register)
        Control.div [] [
            Button.button 
                [ Button.Color IsPrimary; Button.IsFullWidth; Button.OnClick (fun _ -> dispatch Msg.Register); Button.Disabled(isInProgress) ]
                [ content ]
        ]
    
    let form = 
        div 
            [ ClassName "box"] 
            [
                Heading.h1 [ ] [ str (Locale.toTitleCz Registration) ]

                (SharedView.serverErrorToViewIfAny state.RegistrationResult)

                lbl FirstName
                regInput state.Account.FirstName Input.text ChangeFirstName FirstName
                
                lbl LastName
                regInput state.Account.LastName Input.text ChangeLastName LastName

                lbl Email
                regInput state.Account.Email Input.email ChangeEmail Email

                lbl Password
                regInput state.Account.Password Input.password ChangePassword Password
                
                lbl SecondPassword
                regInput state.Account.SecondPassword Input.password ChangeSecondPassword SecondPassword

                btn state.IsRegistrating

                a [ Href <| Router.Page.Auth(Router.AuthPage.Login).ToPath(); OnClick Router.goToUrl] [
                    str (Text.TextValue.BackToLogin |> Locale.toTitleCz)
                ]   
            ]

    let content = 
        match state.RegistrationResult with
        | None | Some (Error _) -> form
        | Some (Ok _) -> TextMessageValue.RegistrationSuccessful |> Locale.toCzMsg |> str |> SharedView.successBox

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