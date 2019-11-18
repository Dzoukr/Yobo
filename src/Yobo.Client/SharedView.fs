module Yobo.Client.SharedView

open Yobo.Shared.Communication
open Fable.React
open Fable.React.Props
open Yobo.Shared
open Thoth.Elmish
open System
open System.Globalization
open Fulma

let errorBox content =
    article [ ClassName "message is-danger has-text-centered" ] [
        div [ ClassName "message-body"][
            i [ ClassName "fa fa-exclamation-circle"; Style [ PaddingRight 10 ] ] []
            content
        ]
    ]

let successBox content =
    article [ ClassName "message is-success has-text-centered" ] [
        div [ ClassName "message-body"][
            i [ ClassName "fa fa-check-circle"; Style [ PaddingRight 10 ] ] []
            content
        ]
    ]

let inProgressBox content =
    article [ ClassName "message is-info has-text-centered" ] [
        div [ ClassName "message-body"][
            i [ ClassName "fas fa-circle-notch fa-spin" ] []
            content
        ]
    ]

let infoBox content =
    article [ ClassName "message is-info has-text-centered" ] [
        div [ ClassName "message-body"][
            i [ ClassName "fa fa-info-circle"; Style [ PaddingRight 10 ] ] []
            content
        ]
    ]

let warningBox content =
    article [ ClassName "message is-warning has-text-centered" ] [
        div [ ClassName "message-body"][
            i [ ClassName "fa fa-exclamation-triangle"; Style [ PaddingRight 10 ] ] []
            content
        ]
    ]

let serverErrorToView (serverError:ServerError) =
    serverError.Explain() |> str |> errorBox

let serverErrorToToast (serverError:ServerError) =
    serverError.Explain()
    |> Toast.message
    |> Toast.title "Došlo k chybě"
    |> Toast.position Toast.TopCenter
    |> Toast.noTimeout
    |> Toast.withCloseButton
    |> Toast.error

let successToast msg =
    msg
    |> Toast.message
    |> Toast.position Toast.TopCenter
    |> Toast.success

let resultToToast successMsg (res:Result<_,ServerError>) =
    match res with
    | Ok _ -> successMsg |> successToast
    | Error err -> err |> serverErrorToToast

let resultToView successView (res:Result<_,ServerError>) =
    match res with
    | Ok v -> v |> successView
    | Error er -> er |> serverErrorToView

let serverErrorToViewIfAny (res:Result<_,ServerError> option) =
    match res with
    | Some (Error er) -> er |> serverErrorToView
    | _ -> str ""

let serverResultToViewIfAny successView (res:ServerResult<_> option) =
    res
    |> Option.map (resultToView successView)
    |> Option.defaultValue (str "")


let toCzDate (date:DateTimeOffset) = date.ToString("dd. MM. yyyy")
let toCzTime (date:DateTimeOffset) = date.ToString("HH:mm")
let toCzDateTime (date:DateTimeOffset) = date.ToString("dd. MM. yyyy HH:mm")

let termsModal isActive closeDisplay =
    Modal.modal [ Modal.IsActive isActive ] [
        Modal.background [ Props [ OnClick closeDisplay ] ] [ ]
        Modal.Card.card [ ] [
            Modal.Card.head [ ] [
                Modal.Card.title [ ] [ str "Obchodní podmínky" ]
                Delete.delete [ Delete.OnClick closeDisplay ] [ ]
            ]
            Modal.Card.body [ GenericOption.Props [ DangerouslySetInnerHTML { __html = StaticText.terms } ] ][ ]
        ]
    ]

module Forms =
    open Yobo.Shared.Validation
    open Fable.Core.JsInterop

    let private errorAndColor (res:ValidationResult) name =
        let error = res |> Validation.tryGetFieldError name
        let clr = if error.IsSome then Input.Color IsDanger else Input.Option.Props []
        let help = if error.IsSome then
                    Help.help [ Help.Color IsDanger ]
                        [ str (error.Value.ErrorType.Explain()) ]
                   else span [] []
        help,clr

    let private genericControl typ (res:ValidationResult) name getter setter =
        let help,clr = errorAndColor res name
        Control.div [] [
            typ [
                clr
                Input.Option.Value getter
                Input.Option.OnChange (fun e -> !!e.target?value |> setter)
            ]
            help
        ]

    let text (res:ValidationResult) name getter setter = genericControl Input.text res name getter setter
    let password (res:ValidationResult) name getter setter = genericControl Input.password res name getter setter
