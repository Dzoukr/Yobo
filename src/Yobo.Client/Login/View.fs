module Yobo.Client.Login.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Core.JsInterop
open Yobo.Client
open Yobo.Client.Login.Domain
open Fable.Import


let render (state : State) (dispatch : Msg -> unit) =
    
    let pwd = 
        Control.div [] [
            Input.password [ Input.Option.Placeholder "Vaše heslo"; Input.Option.OnChange (fun e -> !!e.target?value |> ChangePassword |> dispatch) ]
        ]

    let email =
        Control.div [] [
            Input.text [ Input.Option.Placeholder "Váš email"; Input.Option.OnChange (fun e -> !!e.target?value |> ChangeEmail |> dispatch) ]
        ]

    let btn isLogging =
        let content = if isLogging then i [ ClassName "fa fa-circle-o-notch fa-spin" ] [] else str "Přihlásit se"
        Control.div [] [
            Button.button 
                [ Button.Color IsPrimary; Button.IsFullWidth; Button.OnClick (fun _ -> dispatch Login)  ]
                [ content  ]
        ]

    let footer = 
        div [] [
            a [ Href <| Router.Page.Registration.ToPath(); OnClick Router.goToUrl] [
                str "Registrace"
            ]
            str " · "
            a [ Href <| Router.Page.ForgottenPassword.ToPath(); OnClick Router.goToUrl] [
                str "Zapomněl(a) jsem heslo!"
            ]
        ]
    
    
    let form = 
        div 
            [ ClassName "box"] 
            [
                Heading.h1 [ ] [ str "Yoga Booking" ]

                (SharedView.serverErrorToViewIfAny state.LoginResult)

                email
                pwd
                btn state.IsLogging
                footer
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