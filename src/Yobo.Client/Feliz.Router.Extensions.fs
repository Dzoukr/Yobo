namespace Feliz.Router

open Browser.Types
open Feliz.Router
open Fable.Core.JsInterop

module Router = 
    let goToUrl (e: MouseEvent) =
        e.preventDefault()
        let href : string = !!e.currentTarget?attributes?href?value
        Router.navigatePath href |> List.map (fun f -> f ignore) |> ignore