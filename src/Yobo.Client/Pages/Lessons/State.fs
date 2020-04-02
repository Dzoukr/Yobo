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
    | Init -> { Model.init with WeekOffset = model.WeekOffset }, Cmd.ofMsg <| WeekOffsetChanged 0
    | SelectActiveForm v -> { model with ActiveForm = v }, Cmd.none
    | ToggleDate d ->
        let newDates =
            if model.SelectedDates |> List.contains d then
                model.SelectedDates |> List.filter ((<>) d)
            else
                d :: model.SelectedDates
            |> List.sort
        
        let cmd = if newDates.Length = 0 then Cmd.ofMsg <| SelectActiveForm None else Cmd.none                
        { model with
            SelectedDates = newDates
            LessonsForm = model.LessonsForm |> ValidatedForm.updateWithFn (fun x -> { x with Dates = newDates })
            WorkshopsForm = model.WorkshopsForm |> ValidatedForm.updateWithFn (fun x -> { x with Dates = newDates })
            }, cmd
    | WeekOffsetChanged o ->
        { model with WeekOffset = o }, [ LoadLessons; LoadWorkshops; LoadOnlineLessons ] |> List.map Cmd.ofMsg |> Cmd.batch
    | LessonsFormChanged f ->
        { model
            with LessonsForm =
                    model.LessonsForm
                    |> ValidatedForm.updateWith f
                    |> ValidatedForm.validateWithIfSent validateCreateLessons }, Cmd.none
    | OnlineLessonsFormChanged f ->
        { model
            with OnlinesForm =
                    model.OnlinesForm
                    |> ValidatedForm.updateWith f
                    |> ValidatedForm.validateWithIfSent validateCreateOnlineLessons }, Cmd.none
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
    | CreateOnlineLessons ->
        let model =
            { model with
                OnlinesForm = model.OnlinesForm
                              |> ValidatedForm.validateWith validateCreateOnlineLessons
                              |> ValidatedForm.markAsSent
            }
        if model.OnlinesForm |> ValidatedForm.isValid then
            { model with OnlinesForm = model.OnlinesForm |> ValidatedForm.startLoading },
                Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.CreateOnlineLessons)) model.OnlinesForm.FormData OnlineLessonsCreated
        else model, Cmd.none
    | OnlineLessonsCreated res ->
        let model = { model with OnlinesForm = model.OnlinesForm |> ValidatedForm.stopLoading; SelectedDates = [] }
        match res with
        | Ok _ -> model, Cmd.batch [ ServerResponseViews.showSuccessToast "Online lekce úspěšně přidány."; Cmd.ofMsg Init ]
        | Error e -> model, e |> ServerResponseViews.showErrorToast        
    | WorkshopsFormChanged f ->
        { model
            with WorkshopsForm =
                    model.WorkshopsForm
                    |> ValidatedForm.updateWith f
                    |> ValidatedForm.validateWithIfSent validateCreateWorkshops }, Cmd.none
    
    | LoadLessons ->
        let pars = model.WeekOffset |> DateRange.getDateRangeForWeekOffset
        { model with LessonsForm = model.LessonsForm |> ValidatedForm.startLoading }, Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.GetLessons)) pars LessonsLoaded
    | LessonsLoaded res ->
        let model = { model with LessonsForm = model.LessonsForm |> ValidatedForm.stopLoading }
        match res with
        | Ok lsns -> { model with Lessons = lsns }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast
    | LoadOnlineLessons ->
        let pars = model.WeekOffset |> DateRange.getDateRangeForWeekOffset
        { model with OnlinesForm = model.OnlinesForm |> ValidatedForm.startLoading }, Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.GetOnlineLessons)) pars OnlineLessonsLoaded
    | OnlineLessonsLoaded res ->
        let model = { model with OnlinesForm = model.OnlinesForm |> ValidatedForm.stopLoading }
        match res with
        | Ok v -> { model with Onlines = v }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast
    | LoadWorkshops ->
        let pars = model.WeekOffset |> DateRange.getDateRangeForWeekOffset
        { model with WorkshopsForm = model.WorkshopsForm |> ValidatedForm.startLoading }, Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.GetWorkshops)) pars WorkshopsLoaded
    | WorkshopsLoaded res ->
        let model = { model with WorkshopsForm = model.WorkshopsForm |> ValidatedForm.stopLoading }
        match res with
        | Ok v -> { model with Workshops = v }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast
    | CreateWorkshops ->
        let model =
            { model with
                WorkshopsForm = model.WorkshopsForm
                              |> ValidatedForm.validateWith validateCreateWorkshops
                              |> ValidatedForm.markAsSent
            }
        if model.WorkshopsForm |> ValidatedForm.isValid then
            { model with WorkshopsForm = model.WorkshopsForm |> ValidatedForm.startLoading },
                Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.CreateWorkshops)) model.WorkshopsForm.FormData WorkshopsCreated
        else model, Cmd.none
    | WorkshopsCreated res ->
        let model = { model with WorkshopsForm = model.WorkshopsForm |> ValidatedForm.stopLoading; SelectedDates = [] }
        match res with
        | Ok _ -> model, Cmd.batch [ ServerResponseViews.showSuccessToast "Workshopy úspěšně přidány."; Cmd.ofMsg Init ]
        | Error e -> model, e |> ServerResponseViews.showErrorToast
    | SelectLesson i ->
        { model with ActiveDetailForm = i |> LessonDetailForm |> Some }, Cmd.none