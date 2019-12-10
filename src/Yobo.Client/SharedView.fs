module Yobo.Client.SharedView

open Feliz
open Feliz.Bulma

module ValidationViews =

    open Yobo.Shared.Validation

    let help errors name =
        errors
        |> List.tryFind (fun x -> x.Field = name)
        |> Option.map (fun x ->
            Bulma.help [
                help.isDanger
                prop.text (x.Type |> ValidationErrorType.explain)
            ]
        )
        |> Option.defaultValue Html.none

    let color errors name =
        errors
        |> List.tryFind (fun x -> x.Field = name)
        |> Option.map (fun _ -> input.isDanger)
        |> Option.defaultValue (Interop.mkAttr "dummy" "")