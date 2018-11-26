module Yobo.Client.ActivateAccount.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Core.JsInterop

open Yobo.Client
open Yobo.Client.ActivateAccount.Domain



let render activationId (state : State) (dispatch : Msg -> unit) =
    div [] [
        str "Tady je aktivace"
    ]