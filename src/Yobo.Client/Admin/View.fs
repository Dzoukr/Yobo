module Yobo.Client.Admin.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open System
open Yobo.Client.Admin.Domain

let render (state : State) (dispatch : Msg -> unit) =

    div [] [
        state.Users |> string |> str
    ]