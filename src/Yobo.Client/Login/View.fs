module Yobo.Client.Login.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Core.JsInterop
open Yobo.Client
open Yobo.Client.Login.Domain
open Fable.Import
open Yobo.Shared.Auth
open Yobo.Shared.Communication
open Yobo.Shared

let render (state : State) (dispatch : Msg -> unit) =
    
    let pwd = 
        Control.div [] [
            Input.password [ Input.Option.Placeholder (Text.TextValue.Password |> Locale.toTitleCz); Input.Option.OnChange (fun e -> !!e.target?value |> ChangePassword |> dispatch) ]
        ]

    let email =
        Control.div [] [
            Input.text [ Input.Option.Placeholder (Text.TextValue.Email |> Locale.toTitleCz); Input.Option.OnChange (fun e -> !!e.target?value |> ChangeEmail |> dispatch) ]
        ]

    let btn isLogging =
        let content = if isLogging then i [ ClassName "fa fa-circle-o-notch fa-spin" ] [] else str (Text.TextValue.Login |> Locale.toTitleCz)
        Control.div [] [
            Button.button 
                [ Button.Color IsPrimary; Button.IsFullWidth; Button.OnClick (fun _ -> dispatch Login)  ]
                [ content  ]
        ]

    let footer = 
        div [] [
            a [ Href <| Router.Page.Registration.ToPath(); OnClick Router.goToUrl] [
                str (Text.TextValue.Registration |> Locale.toTitleCz)
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
                    Text.TextMessageValue.ActivationLinkResendError |> Locale.toCzMsg |> str
                | Some (Ok _) ->
                    Text.TextMessageValue.ActivationLinkSuccessfullyResent |> Locale.toCzMsg |> str
                | None ->
                    Button.button [ Button.Color IsInfo; Button.OnClick (fun _ -> id |> ResendActivation |> dispatch )] [
                        Text.TextValue.ResendActivationLink |> Locale.toTitleCz |> str
                    ]

            span [] [
                Text.TextMessageValue.AccountNotActivatedYet |> Locale.toCzMsg |> str
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