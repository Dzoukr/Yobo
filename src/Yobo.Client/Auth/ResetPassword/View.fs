module Yobo.Client.Auth.ResetPassword.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Yobo.Client
open Yobo.Shared.Auth
open Yobo.Shared
open Domain
open FSharp.Rop

let render (state : State) (dispatch : Msg -> unit) =

    let pwd1 =
        SharedView.Forms.password
            state.ValidationResult
            "Password"
            state.Form.Password
            (fun v -> { state.Form with Password = v } |> FormChanged |> dispatch)

    let pwd2 =
        SharedView.Forms.password
            state.ValidationResult
            "SecondPassword"
            state.Form.SecondPassword
            (fun v -> { state.Form with SecondPassword = v } |> FormChanged |> dispatch)
    
    let btn =
        Control.div [] [
            Button.button 
                [ Button.Color IsPrimary; Button.IsFullWidth; Button.OnClick (fun _ -> dispatch ResetPassword)  ]
                [ str "Změnit heslo"  ]
        ]

    let successMsg =
        span [] [
            str "Vaše heslo bylo úspěšně změněno."
            div [] [
                a [ Href Router.Routes.login; OnClick Router.goToUrl] [
                    str "Zpátky na přihlášení"
                ]
            ]
        ] |> SharedView.successBox

    let displayIf cond controls =
        if cond then controls else str ""

    let form = 
        div 
            [ ClassName "box"] 
            [
                Heading.h1 [ ] [ str "Změna hesla" ]
                (SharedView.serverResultToViewIfAny (fun _ -> successMsg) state.ResetResult)

                displayIf (state.ResetResult.IsNone || Result.isError state.ResetResult.Value) (div [] [
                    Field.div [] [ pwd1 ]
                    Field.div [] [ pwd2 ]
                    Field.div [] [ btn ]
                ])
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