module Yobo.Client.Pages.Calendar.State

open Domain
open Elmish
open Yobo.Shared.Auth.Validation
open Yobo.Client.Server
open Yobo.Client.SharedView
open Yobo.Client.StateHandlers
open Yobo.Shared.Auth.Communication
open Yobo.Client.Forms

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | Init -> { Model.init with WeekOffset = model.WeekOffset }, Cmd.ofMsg <| WeekOffsetChanged model.WeekOffset
    | WeekOffsetChanged o -> { model with WeekOffset = o }, [ LoadLessons; LoadOnlineLessons ] |> List.map Cmd.ofMsg |> Cmd.batch
    | LoadLessons -> model, Cmd.OfAsync.eitherAsResult (onReservationsService (fun x -> x.GetLessons)) model.WeekOffset LessonsLoaded
    | LessonsLoaded res ->
        match res with
        | Ok lsns -> { model with Lessons = lsns }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast
    | LoadOnlineLessons -> model, Cmd.OfAsync.eitherAsResult (onReservationsService (fun x -> x.GetOnlineLessons)) model.WeekOffset OnlineLessonsLoaded
    | OnlineLessonsLoaded res ->
        match res with
        | Ok lsns -> { model with OnlineLessons = lsns }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast
            
        