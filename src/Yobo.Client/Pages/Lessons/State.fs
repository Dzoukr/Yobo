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
    | Init -> { Model.init with WeekOffset = model.WeekOffset }, Cmd.batch [ Cmd.ofMsg LoadLessons ]
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
        { model
            with LessonsForm =
                    model.LessonsForm
                    |> ValidatedForm.updateWith f
                    |> ValidatedForm.updateWithFn (fun x -> { x with Dates = model.SelectedDates })
                    |> ValidatedForm.validateWithIfSent validateCreateLessons }, Cmd.none
    | CreateLessons ->
        let model =
            { model with
                LessonsForm = model.LessonsForm
                              |> ValidatedForm.validateWith validateCreateLessons
                              |> ValidatedForm.markAsSent
            }
        if model.LessonsForm |> ValidatedForm.isValid then
            { model with LessonsForm = model.LessonsForm |> ValidatedForm.startLoading },
                Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.CreateLessons)) model.LessonsForm.FormData LessonsCreated
        else model, Cmd.none
    | LessonsCreated res ->
        let model = { model with LessonsForm = model.LessonsForm |> ValidatedForm.stopLoading; SelectedDates = [] }
        match res with
        | Ok _ -> model, Cmd.batch [ ServerResponseViews.showSuccessToast "Lekce úspěšně přidány."; Cmd.ofMsg Init ]
        | Error e -> model, e |> ServerResponseViews.showErrorToast
    | LoadLessons ->
        let pars = model.WeekOffset |> DateRange.getDateRangeForWeekOffset
        { model with LessonsLoading = true }, Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.GetLessons)) pars LessonsLoaded
    | LessonsLoaded res ->
        let model = { model with LessonsLoading = false }
        match res with
        | Ok lsns -> { model with Lessons = lsns }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast