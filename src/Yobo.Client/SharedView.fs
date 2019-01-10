module Yobo.Client.SharedView

open Yobo.Shared.Communication
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Yobo.Shared
open Thoth.Elmish

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
            i [ ClassName "fa fa-circle-o-notch fa-spin"; Style [ PaddingRight 10 ] ] [] 
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

let serverErrorToView (serverError:ServerError) =
    serverError |> Locale.serverErrorToCz |> str |> errorBox

let serverErrorToToast (serverError:ServerError) =
    serverError
    |> Locale.serverErrorToCz
    |> Toast.message
    |> Toast.title (Text.TextMessageValue.ErrorOccured |> Locale.toCzMsg)
    |> Toast.position Toast.TopCenter
    |> Toast.noTimeout
    |> Toast.withCloseButton
    |> Toast.error

let successToast msg =
    msg
    |> Toast.message
    |> Toast.position Toast.TopCenter
    |> Toast.success

let resultToView successView (res:Result<_,ServerError>) =
    match res with
    | Ok v -> v |> successView
    | Error er -> er |> serverErrorToView

let serverErrorToViewIfAny (res:Result<_,ServerError> option) =
    match res with
    | Some (Error er) -> er |> serverErrorToView
    | _ -> str ""