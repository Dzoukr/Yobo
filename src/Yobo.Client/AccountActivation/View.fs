module Yobo.Client.AccountActivation.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma

open Yobo.Client.AccountActivation.Domain

let render activationId (state : State) (dispatch : Msg -> unit) =

    let content = div [] [
        str "Tady je aaa"
        str (state.ToString())
    ]

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