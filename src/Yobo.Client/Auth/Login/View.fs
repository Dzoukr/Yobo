module Yobo.Client.Auth.Login.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Core.JsInterop
open Yobo.Client
open Yobo.Client.Auth.Login.Domain
open Fable.Import
open Yobo.Shared.Auth
open Yobo.Shared.Communication
open Yobo.Shared

let render (state : State) (dispatch : Msg -> unit) =
    
    let pwd = 
        Control.div [] [
            Input.password [
                Input.Option.Placeholder "Vaše heslo"
                Input.Option.OnChange (fun e -> !!e.target?value |> ChangePassword |> dispatch) ]
        ]

    let email =
        Control.div [] [
            Input.text [
                Input.Option.Value state.Login.Email
                Input.Option.Placeholder "Váš email"
                Input.Option.OnChange (fun e -> !!e.target?value |> ChangeEmail |> dispatch) ]
        ]

    let btn isLogging =
        let content = if isLogging then i [ ClassName "fa fa-circle-notch fa-spin" ] [] else str "Přihlásit se"
        Control.div [] [
            Button.button 
                [ Button.Color IsPrimary; Button.IsFullWidth; Button.OnClick (fun _ -> dispatch Login)  ]
                [ content  ]
        ]

    let footer = 
        div [] [
            a [ Href <| Router.Page.Auth(Router.AuthPage.Registration).ToPath(); OnClick Router.goToUrl] [
                str "Nemáte ještě účet? Zaregistrujte se."
            ]
            //str " · "
            //a [ Href <| Router.Page.ForgottenPassword.ToPath(); OnClick Router.goToUrl] [
            //    str "Zapomněl(a) jsem heslo!"
            //]
        ]

    let resendActivationInfoBox =
        match state.LoginResult with
        | Some (Error (ServerError.AuthError (AccountNotActivated id))) ->
            let resendDiv =
                match state.ResendActivationResult with
                | Some (Error _) ->
                    str "Aktivační odkaz se nepodařilo odeslat. Zavřete okno prohlížeče a zkuste to prosím později."
                | Some (Ok _) ->
                    str "Aktivační odkaz byl úspěšně odeslán do vaší emailové schránky."
                | None ->
                    Button.button [ Button.Color IsInfo; Button.OnClick (fun _ -> id |> ResendActivation |> dispatch )] [
                        str "Poslat aktivační odkaz"
                    ]

            span [] [
                str "Zkuste se podívat do vaší emailové schránky, kam jsme vám poslali aktivační odkaz. Pokud ho nemůžete nalézt, klikněte na tlačítko níže a nechte si poslat nový."
                div [ Style [ MarginTop 10]] [
                    resendDiv
                ]
            ] |> SharedView.infoBox
        | _ -> str ""
        
    
    let form = 
        div 
            [ ClassName "box"] 
            [
                Heading.h1 [ ] [ str "Yoga Booking" ]

                (SharedView.serverErrorToViewIfAny state.LoginResult)
                resendActivationInfoBox

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