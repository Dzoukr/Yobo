module Yobo.Client.Pages.AccountActivation.View

open Yobo.Client.Router
open Yobo.Client.SharedView
open Domain
open Yobo.Client
open Feliz
open Feliz.Router
open Feliz.Bulma
open Yobo.Shared.Domain

let view (state:Model) (dispatch : Msg -> unit) =
    
    let successContent (msg:string) =
        Html.span [
            Html.text msg
            Html.div [
                Html.aRouted "Zpátky na přihlášení" Page.Login
            ]
        ]
    
    let content =
        match state.ActivationResult with
        | Some (Ok _) -> successContent "Váš účet byl právě zaktivován. Nyní se můžete přihlásit do systému." |> SharedView.BoxedViews.showSuccess
        | Some (Error e) -> e |> ServerError.explain |> SharedView.BoxedViews.showErrorMessage
        | None -> "Aktivuji váš účet..." |> SharedView.BoxedViews.showInProgressMessage
    
    Bulma.hero [
        Bulma.heroBody [
            Bulma.container [
                prop.children [ content ]
            ]
        ]
    ]