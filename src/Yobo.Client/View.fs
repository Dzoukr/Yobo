module Yobo.Client.View

open Domain
open Elmish
open Feliz
open Feliz.Bulma
open Router
    
let view (model:Model) (dispatch:Msg -> unit) =
    let render =
        match model.CurrentPage with
        | Auth m -> Auth.View.view m (AuthMsg >> dispatch)
        | Calendar ->
            Html.a [
                prop.text "Login"
                prop.onClick (fun _ -> Navigate(Paths.Login) |> dispatch)
            ]
            
    Router.router [
        Router.onUrlChanged (Router.parseUrl >> UrlChanged >> dispatch)
        Router.pathMode
        Router.application render
    ]