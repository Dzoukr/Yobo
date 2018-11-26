module Yobo.Client.AccountActivation.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Core.JsInterop

open Yobo.Client.AccountActivation.Domain


let render activationId (state : State) (dispatch : Msg -> unit) =
    div [] [
        str "Tady je aktivace"
    ]