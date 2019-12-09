module Yobo.Client.View

open Domain
open Feliz
open Feliz.Bulma
open Feliz.Router
    
let view (model : Model) (dispatch : Msg -> unit) =
    let render =
        Html.div "AHOJ"
    Router.router [
        Router.onUrlChanged (Router.parseUrl >> UrlChanged >> dispatch)
        Router.application render
    ]