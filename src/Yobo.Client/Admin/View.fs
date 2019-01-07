module Yobo.Client.Admin.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open System
open Yobo.Client.Admin.Domain
open Yobo.Shared

let render (state : State) (dispatch : Msg -> unit) =
    let rows = state.Users |> List.map (fun u ->
        tr [] [
            td [] [ str u.LastName]
            td [] [ str u.FirstName]
            td [] [ str u.Email]
            td [] [ str (string u.ActivatedUtc)]
        ]
    )
    Table.table [ Table.IsHoverable ]
        [
            thead [ ]
                [ tr [ ] [
                    th [ ] [ str (Text.TextValue.LastName |> Locale.toTitleCz) ]
                    th [ ] [ str (Text.TextValue.FirstName |> Locale.toTitleCz) ]
                    th [ ] [ str (Text.TextValue.Email |> Locale.toTitleCz) ]
                    th [ ] [ str "AAA" ]
                    ]
                ] 
            tbody [ ] rows
        ]