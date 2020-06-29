module Yobo.Client.Pages.AccountActivation.View

open System
open Yobo.Client.Router
open Yobo.Client.SharedView
open Domain
open Yobo.Client
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Yobo.Shared.Errors

let view = React.functionComponent(fun (props:{|key:Guid|}) ->
    let model, _ = React.useElmish(State.init props.key, State.update, [| |])
    
    let successContent (msg:string) =
        Html.span [
            Html.text msg
            Html.div [
                Html.aRouted "Zpátky na přihlášení" (Anonymous Login)
            ]
        ]
    
    let content =
        match model.ActivationResult with
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
)