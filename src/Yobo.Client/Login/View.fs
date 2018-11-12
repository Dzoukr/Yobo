module Yobo.Client.Login.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Yobo.Client.Login.Domain
open Fulma.FontAwesome

let show = function
| { Counter = Some x } -> string x
| { Counter = None   } -> "Loading..."

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let loginBox =
    form [] [
        // Email field
        Field.div [ ]
            [ Label.label [ ]
                [ str "Email" ]
              Control.div [ Control.HasIconLeft
                            Control.HasIconRight ]
                [ Input.email [ Input.Color IsDanger
                                Input.DefaultValue "hello@" ]
                  Icon.faIcon [ Icon.Size IsSmall; Icon.IsLeft ] [ Fa.icon Fa.I.Envelope ]
                  Icon.faIcon [ Icon.Size IsSmall; Icon.IsRight ] [ Fa.icon Fa.I.Warning ] ]
              Help.help [ Help.Color IsDanger ]
                [ str "This email is invalid" ] ]
        // Email field
        Field.div [ ]
            [ Label.label [ ]
                [ str "Heslo" ]
              Control.div [ Control.HasIconLeft
                            Control.HasIconRight ]
                [ Input.password [ Input.Color IsDanger ]
                  Icon.faIcon [ Icon.Size IsSmall; Icon.IsLeft ] [ Fa.icon Fa.I.Key ]
                  Icon.faIcon [ Icon.Size IsSmall; Icon.IsRight ] [ Fa.icon Fa.I.Warning ] ]
              Help.help [ Help.Color IsDanger ]
                [ str "This password must be filled" ] ]
        
        // Control area (submit, cancel, etc.)
        Field.div [ ]
            [ Control.div [ Control.Option.Modifiers [Modifier.TextAlignment (Screen.All, TextAlignment.Centered )] ]
                [ Button.button [ Button.Color IsPrimary ]
                    [ str "Submit" ] ]
            ]

    ]

let render (model : State) (dispatch : Msg -> unit) =
    div [] [ loginBox ]
    
    
    // div []
    //     [ Navbar.navbar [ Navbar.Color IsPrimary ]
    //         [ Navbar.Item.div [ ]
    //             [ Heading.h2 [ ]
    //                 [ str "LOGIN Template" ] ] ]

    //       Container.container []
    //           [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
    //                 [ Heading.h3 [] [ str ("Press buttons to manipulate counter: " + show model) ] ]
    //             Columns.columns []
    //                 [ Column.column [] [ button "-" (fun _ -> dispatch Decrement) ]
    //                   Column.column [] [ button "+" (fun _ -> dispatch Increment) ] 
    //                   Column.column [] [ button "reload" (fun _ -> dispatch Reset) ] 
    //                 ] 
    //           ]
    //     ]