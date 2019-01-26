module Yobo.Client.SharedView

open Yobo.Shared.Communication
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Yobo.Shared
open Thoth.Elmish
open System
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

let toCzDate (date:DateTimeOffset) = date.ToString("dd. MM. yyyy")
let toCzTime (date:DateTimeOffset) = date.ToString("HH:mm")
let rulesModal isActive closeDisplay =
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

             