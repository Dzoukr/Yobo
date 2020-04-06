module Yobo.Client.Pages.MyAccount.State

open Yobo.Client.Router
open Domain
open Elmish
open Fable.Core
open Feliz.Router
open Yobo.Client
open Yobo.Shared.Auth.Validation
open Yobo.Client.Server
open Yobo.Client.SharedView
open Yobo.Shared.Auth.Communication
open Yobo.Client.Forms

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | Init -> model, ([LoadMyLessons; LoadMyOnlineLessons] |> List.map Cmd.ofMsg |> Cmd.batch)
    | LoadMyLessons -> model, Cmd.OfAsync.eitherAsResult (onUserAccountService (fun x -> x.GetMyLessons)) () MyLessonsLoaded
    | LoadMyOnlineLessons -> model, Cmd.OfAsync.eitherAsResult (onUserAccountService (fun x -> x.GetMyOnlineLessons)) () MyOnlineLessonsLoaded
    | MyLessonsLoaded res ->
        match res with
        | Ok lsn -> { model with Lessons = lsn }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast
    | MyOnlineLessonsLoaded res ->
        match res with
        | Ok lsn -> { model with OnlineLessons = lsn }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast
    
    