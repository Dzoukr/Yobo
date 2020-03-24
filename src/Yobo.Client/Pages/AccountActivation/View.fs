module Yobo.Client.Pages.AccountActivation.View

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
                Html.a [ prop.text "Zpátky na přihlášení"; prop.href (Router.formatPath Paths.Login); prop.onClick Router.goToUrl ]
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