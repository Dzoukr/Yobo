module Yobo.Client.Login.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Yobo.Client.Login.Domain
open Fable.Core.JsInterop


// // let button txt onClick =
// //     Button.button
// //         [ Button.IsFullWidth
// //           Button.Color IsPrimary
// //           Button.OnClick onClick ]
// //         [ str txt ]

// // let loginBox =
// //     form [] [
// //         // Email field
// //         Field.div [ ]
// //             [ Label.label [ ]
// //                 [ str "Email" ]
// //               Control.div [ Control.HasIconLeft
// //                             Control.HasIconRight ]
// //                 [ Input.email [ Input.Color IsDanger
// //                                 Input.DefaultValue "hello@" ]
// //                   Icon.faIcon [ Icon.Size IsSmall; Icon.IsLeft ] [ Fa.icon Fa.I.Envelope ]
// //                   Icon.faIcon [ Icon.Size IsSmall; Icon.IsRight ] [ Fa.icon Fa.I.Warning ] ]
// //               Help.help [ Help.Color IsDanger ]
// //                 [ str "This email is invalid" ] ]
// //         // Email field
// //         Field.div [ ]
// //             [ Label.label [ ]
// //                 [ str "Heslo" ]
// //               Control.div [ Control.HasIconLeft
// //                             Control.HasIconRight ]
// //                 [ Input.password [ Input.Color IsDanger ]
// //                   Icon.faIcon [ Icon.Size IsSmall; Icon.IsLeft ] [ Fa.icon Fa.I.Key ]
// //                   Icon.faIcon [ Icon.Size IsSmall; Icon.IsRight ] [ Fa.icon Fa.I.Warning ] ]
// //               Help.help [ Help.Color IsDanger ]
// //                 [ str "This password must be filled" ] ]
        
// //         // Control area (submit, cancel, etc.)
// //         Field.div [ ]
// //             [ Control.div [ Control.Option.Modifiers [Modifier.TextAlignment (Screen.All, TextAlignment.Centered )] ]
// //                 [ Button.button [ Button.Color IsPrimary ]
// //                     [ str "Submit" ] ]
// //             ]

// //     ]

// let appIcon = 
//   img [ Src "/img/fable_logo.png"
//         Style [ Height 80; Width 100 ] ]


let render (state : State) (dispatch : Msg -> unit) =
    
    let pwd = 
        Control.div [] [
            Input.password [ Input.Option.Placeholder "Vaše heslo"; Input.Option.OnChange (fun e -> !!e.target?value |> PasswordChange |> dispatch) ]
        ]

    let email =
        Control.div [] [
            Input.text [ Input.Option.Placeholder "Váš email"; Input.Option.OnChange (fun e -> !!e.target?value |> EmailChange |> dispatch) ]
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
            a [ Href <| Router.Page.Register.ToHash()] [
                str "Registrace"
            ]
            str " · "
            a [ Href "#neeee"] [
                str "Zapomněl(a) jsem heslo!"
            ]
        ]
    
    let form = 
        div 
            [ ClassName "box"] 
            [
                Heading.h1 [ ] [ str "Yoga Booking" ]
                email
                pwd
                btn state.IsLogging
                footer
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