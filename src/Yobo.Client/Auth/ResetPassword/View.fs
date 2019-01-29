module Yobo.Client.Auth.ResetPassword.View

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

    let formChanged setter v =
        v |> setter |> FormChanged |> dispatch

    let pwd1 =
        Control.div [] [
            Input.password [
                Input.Option.Value state.Form.Password
                Input.Option.OnChange (fun e -> !!e.target?value |> formChanged (fun v -> { state.Form with Password = v })) ]
        ]
    let pwd2 =
        Control.div [] [
            Input.password [
                Input.Option.Value state.Form.PasswordAgain
                Input.Option.OnChange (fun e -> !!e.target?value |> formChanged (fun v -> { state.Form with PasswordAgain = v })) ]
        ]

    let btn =
        Control.div [] [
            Button.button 
                [ Button.Color IsPrimary; Button.IsFullWidth; Button.OnClick (fun _ -> dispatch ResetPassword)  ]
                [ str "Změnit heslo"  ]
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