module Yobo.Client.Pages.Lessons.State

open Domain
open Elmish
open Yobo.Client.Server
open Yobo.Client.Forms
open Yobo.Client.SharedView
open Yobo.Shared.DateTime
open Yobo.Shared.Core.Admin.Validation

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    match msg with
    | ShowForm v -> { model with FormShown = v }, Cmd.none
    | ToggleDate d ->
        let newDates =
            if model.SelectedDates |> List.contains d then
                model.SelectedDates |> List.filter ((<>) d)
            else
                d :: model.SelectedDates
        let cmd = if newDates.Length = 0 then Cmd.ofMsg <| ShowForm(false) else Cmd.none                
        { model with SelectedDates = newDates |> List.sort }, cmd
    | WeekOffsetChanged o ->
        { model with WeekOffset = o }, [ LoadLessons ] |> List.map Cmd.ofMsg |> Cmd.batch
    | LessonsFormChanged f ->
        { model with LessonsForm = model.LessonsForm |> ValidatedForm.updateWith f }, Cmd.none
    | LoadLessons ->
        let pars = model.WeekOffset |> DateRange.getDateRangeForWeekOffset
        { model with LessonsLoading = true }, Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.GetLessons)) pars LessonsLoaded
    | LessonsLoaded res ->
        let model = { model with LessonsLoading = false }
        match res with
        | Ok lsns -> { model with Lessons = lsns }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast