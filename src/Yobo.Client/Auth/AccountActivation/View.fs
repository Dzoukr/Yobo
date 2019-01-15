module Yobo.Client.Auth.AccountActivation.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Yobo.Client.Auth.AccountActivation.Domain
open Yobo.Client
open Yobo.Shared
open System

let render (state : State) (dispatch : Msg -> unit) =
    
    let successContent msg = span [] [
        str msg
        div [] [
            a [ Href <| Router.Page.Auth(Router.AuthPage.Login).ToPath(); OnClick Router.goToUrl] [
                str "Zpátky na přihlášení"
            ]
        ]
    ]
    
    let content =
        match state.ActivationResult with
        | Some (Ok _) -> successContent "Váš účet byl právě zaktivován. Nyní se můžete přihlásit do systému." |> SharedView.successBox
        | Some (Error e) -> e |> SharedView.serverErrorToView
        | None -> div [] [ str "Aktivuji váš účet..." ] |> SharedView.inProgressBox
        
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