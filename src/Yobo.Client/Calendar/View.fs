module Yobo.Client.Calendar.View

open Domain

open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open System
open Yobo.Shared
open Fulma.Extensions.Wikiki
open Yobo.Client

let render (state : State) (dispatch : Msg -> unit) =
    div [] [
        str "TEST"
    ]