module Yobo.Client.SharedView

open Feliz
open Feliz.Bulma
open Yobo.Shared.Communication
open Yobo.Shared.Validation

module ErrorViews =
    open Elmish
    open Elmish.Toastr
    
    let showError (e:ServerError) : Cmd<_> =
        let basicToaster =
            match e with
            | Exception msg ->
                Toastr.message msg
                |> Toastr.title "Došlo k chybě"
            | Validation v ->
                v
                |> List.map (fun x -> x.Field, ValidationErrorType.explain x.Type)
                |> List.map (fun (n,e) -> sprintf "%s : %s" n e)
                |> String.concat "<br/>"
                |> Toastr.message
                |> Toastr.title "Data nejsou vyplněna správně"
                |> Toastr.timeout 30000
                |> Toastr.extendedTimout 10000
            | Authentication e ->
                e
                |> AuthenticationError.explain
                |> Toastr.message
                |> Toastr.timeout 10000
                |> Toastr.extendedTimout 10000
                
        basicToaster
        |> Toastr.position ToastPosition.TopRight
        |> Toastr.hideEasing Easing.Swing
        |> Toastr.withProgressBar
        |> Toastr.showCloseButton
        |> Toastr.error
    

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