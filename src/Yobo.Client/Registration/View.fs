module Yobo.Client.Registration.View

open System
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Core.JsInterop
open Yobo.Client.Registration.Domain
open Yobo.Shared
open Yobo.Shared.Text
open Yobo.Client

let render (state : State) (dispatch : Msg -> unit) =
    let regInput typ msgType txt =
        let error = state.ValidationResult |> Validation.tryGetFieldError txt
        let clr = if error.IsSome then Input.Color IsDanger else Input.Option.Props []
        let help = if error.IsSome then 
                    Help.help [ Help.Color IsDanger ]
                        [ str (error.Value |> Locale.validationErrorToCz ) ]
                   else span [] []
        Control.div [] [
            typ [
                clr
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
                regInput Input.text ChangeFirstName FirstName
                
                lbl LastName
                regInput Input.text ChangeLastName LastName

                lbl Email
                regInput Input.email ChangeEmail Email

                lbl Password
                regInput Input.password ChangePassword Password
                
                lbl SecondPassword
                regInput Input.password ChangeSecondPassword SecondPassword

                btn state.IsRegistrating
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