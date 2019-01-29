module Yobo.Client.Auth.ForgottenPassword.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Core.JsInterop
open Yobo.Client
open Yobo.Shared.Auth
open Yobo.Shared
open Domain
open FSharp.Rop

let render (state : State) (dispatch : Msg -> unit) =
    
    let email =
        Control.div [] [
            Input.text [
                Input.Option.Value state.EmailToReset
                Input.Option.Placeholder "Váš email"
                Input.Option.OnChange (fun e -> !!e.target?value |> EmailChanged |> dispatch) ]
        ]

    let btn =
        Control.div [] [
            Button.button 
                [ Button.Color IsPrimary; Button.IsFullWidth; Button.OnClick (fun _ -> dispatch InitiateReset)  ]
                [ str "Vyresetovat heslo"  ]
        ]

    let successMsg =
        "Na email jsme Vám odeslali odkaz, pomocí kterého si změníte heslo."
        |> str
        |> SharedView.successBox

    let displayIf cond controls =
        if cond then controls else str ""

    let form = 
        div 
            [ ClassName "box"] 
            [
                Heading.h1 [ ] [ str "Reset hesla" ]
                (SharedView.serverResultToViewIfAny (fun _ -> successMsg) state.ResetResult)

                displayIf (state.ResetResult.IsNone || Result.isError state.ResetResult.Value) (div [] [
                    Field.div [] [ email ]
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