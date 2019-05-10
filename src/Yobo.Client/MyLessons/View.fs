module Yobo.Client.MyLessons.View

open Domain
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open System
open Yobo.Shared
open Yobo.Client
open Yobo.Shared.Domain

let render (state : State) (dispatch : Msg -> unit) =
    div [] [
        str "TODO"
    ]