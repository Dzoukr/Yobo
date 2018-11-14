module Yobo.Client.Login.Register.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Core.JsInterop

open Yobo.Client.Login
open Yobo.Client.Login.Register.Domain

let render (state : State) (dispatch : Msg -> unit) =
    
    let txtIn m err =
        let error = state.ValidationResult.TryGetMessage err
        let clr = if error.IsSome then Input.Color IsDanger else Input.Option.Props []
        let help = if error.IsSome then 
                    Help.help [ Help.Color IsDanger ]
                        [ str error.Value ]
                   else span [] []

        Control.div [] [
            Input.text [ clr; Input.Option.OnChange (fun e -> !!e.target?value |> m |> dispatch); ]
            help
        ]

    let pwdIn m =
        Control.div [] [
            Input.password [ Input.Option.OnChange (fun e -> !!e.target?value |> m |> dispatch) ]
        ]

    let lbl s = Label.label [] [ str s ]

    let btn isLogging =
        let content = if isLogging then i [ ClassName "fa fa-circle-o-notch fa-spin" ] [] else str "Zaregistrovat se"
        Control.div [] [
            Button.button 
                [ Button.Color IsPrimary; Button.IsFullWidth; Button.OnClick (fun _ -> dispatch Register)  ]
                [ content  ]
        ]
    
    let form = 
        div 
            [ ClassName "box"] 
            [
                Heading.h1 [ ] [ str "Registrace" ]
                
                lbl "Křestní jméno"
                txtIn ChangeFirstName "FirstName"
                
                lbl "Příjmení"
                txtIn ChangeLastName "LastName"

                lbl "Email"
                txtIn ChangeEmail "Email"

                lbl "Heslo"
                pwdIn ChangePassword
                
                lbl "Heslo (znovu)"
                pwdIn ChangeSecondPassword

                btn state.IsRegistering
                str (state.ToString())
            ]
   
    Hero.hero [ ]
        [ Hero.body [ ]
            [ Container.container 
                [ Container.IsFluid; Container.Props [ ClassName "has-text-centered"] ]
                [ Column.column 
                    [ Column.Width (Screen.All, Column.Is4); Column.Offset (Screen.All, Column.Is4) ] 
                    [ form ] 
                ] 
            ]
        ]