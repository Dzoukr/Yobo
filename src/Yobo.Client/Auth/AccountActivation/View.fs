module Yobo.Client.Auth.AccountActivation.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Yobo.Client.Auth.AccountActivation.Domain
open Yobo.Client
open Yobo.Shared
open Yobo.Shared.Text
open System

let render (state : State) (dispatch : Msg -> unit) =
    
    let successContent msg = span [] [
        str (msg |> Locale.toCzMsg)
        div [] [
            a [ Href <| Router.Page.Auth(Router.AuthPage.Login).ToPath(); OnClick Router.goToUrl] [
                TextValue.Login |> Locale.toTitleCz |> str
            ]
        ]
    ]
    
    let content = 
        match state.ActivationResult with
        | Some (Ok _) -> successContent TextMessageValue.AccountSuccessfullyActivated |> SharedView.successBox
        | Some (Error e) -> e |> SharedView.serverErrorToView
        | None -> div [] [str (TextMessageValue.ActivatingYourAccount |> Locale.toCzMsg)] |> SharedView.inProgressBox
        
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