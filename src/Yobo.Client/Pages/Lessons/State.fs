module Yobo.Client.Pages.Lessons.State

open Domain
open Elmish
open Yobo.Client.Server
open Yobo.Client.Forms
open Yobo.Client.SharedView
open Yobo.Shared.Tuples
open Yobo.Shared.DateTime
open Yobo.Shared.Core.Admin.Validation

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    match msg with
    | Init -> { Model.init with WeekOffset = model.WeekOffset }, Cmd.ofMsg <| WeekOffsetChanged model.WeekOffset
    | SelectActiveForm v -> { model with ActiveForm = v }, Cmd.none
    | ToggleDate d ->
        let newDates =
            if model.SelectedDates |> List.contains d then model.SelectedDates |> List.filter ((<>) d)
            else d :: model.SelectedDates
            |> List.sort
        let cmd = if newDates.Length = 0 then Cmd.ofMsg <| SelectActiveForm None else Cmd.none                
        { model with
            SelectedDates = newDates
            LessonsForm = model.LessonsForm |> ValidatedForm.updateWithFn (fun x -> { x with Dates = newDates })
            WorkshopsForm = model.WorkshopsForm |> ValidatedForm.updateWithFn (fun x -> { x with Dates = newDates })
            OnlinesForm = model.OnlinesForm |> ValidatedForm.updateWithFn (fun x -> { x with Dates = newDates })
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
    | SetActiveLesson l -> { model with ActiveItemModel = l |> Option.map (ActiveLessonModel.init >> ActiveItemModel.Lesson) }, Cmd.none
    | SetActiveWorkshop l -> { model with ActiveItemModel = l |> Option.map (ActiveWorkshopModel.init >> ActiveItemModel.Workshop) }, Cmd.none
    | SetActiveOnlineLesson l -> { model with ActiveItemModel = l |> Option.map (ActiveItemModel.OnlineLesson) }, Cmd.none
    | ActiveItemMsg msg ->
        match msg, model.ActiveItemModel with
        | ActiveLessonMsg m, (Some (Lesson subModel)) ->
            match m with 
            | ChangeLessonDescriptionFromChanged v ->
                ({ subModel with ChangeDescriptionForm = subModel.ChangeDescriptionForm
                                                             |> ValidatedForm.updateWith v
                                                             |> ValidatedForm.validateWithIfSent validateChangeLessonDescription }, Cmd.none)
                |> mapFst (ActiveItemModel.Lesson >> Some >> (fun x -> { model with ActiveItemModel = x }))
            | ChangeLessonDescription ->
                let subModel = { subModel with ChangeDescriptionForm = subModel.ChangeDescriptionForm
                                                             |> ValidatedForm.markAsSent
                                                             |> ValidatedForm.validateWith validateChangeLessonDescription }
                if subModel.ChangeDescriptionForm |> ValidatedForm.isValid then
                    { subModel with ChangeDescriptionForm = subModel.ChangeDescriptionForm |> ValidatedForm.startLoading },
                        Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.ChangeLessonDescription)) subModel.ChangeDescriptionForm.FormData LessonDescriptionChanged
                else subModel, Cmd.none                        
                
                |> mapFst (ActiveItemModel.Lesson >> Some >> (fun x -> { model with ActiveItemModel = x }))
                |> mapSnd (Cmd.map (ActiveLessonMsg >> ActiveItemMsg))
            | LessonDescriptionChanged res ->
                let subModel = { subModel with ChangeDescriptionForm = subModel.ChangeDescriptionForm |> ValidatedForm.stopLoading }
                match res with
                | Ok _ -> subModel, Cmd.batch [ ServerResponseViews.showSuccessToast "Popis úspěšně změněn."; Cmd.ofMsg Init ]
                | Error e -> subModel, e |> ServerResponseViews.showErrorToast
                |> mapFst (ActiveItemModel.Lesson >> Some >> (fun x -> { model with ActiveItemModel = x }))
            | CancelLesson ->
                let subModel = { subModel with CancelLessonForm = subModel.CancelLessonForm
                                                             |> ValidatedForm.markAsSent
                                                             |> ValidatedForm.validateWith validateCancelLesson }
                if subModel.CancelLessonForm |> ValidatedForm.isValid then
                    { subModel with CancelLessonForm = subModel.CancelLessonForm |> ValidatedForm.startLoading },
                        Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.CancelLesson)) subModel.CancelLessonForm.FormData LessonCancelled
                else subModel, Cmd.none
                |> mapFst (ActiveItemModel.Lesson >> Some >> (fun x -> { model with ActiveItemModel = x }))
                |> mapSnd (Cmd.map (ActiveLessonMsg >> ActiveItemMsg))
            | LessonCancelled res ->
                let subModel = { subModel with CancelLessonForm = subModel.CancelLessonForm |> ValidatedForm.stopLoading }
                match res with
                | Ok _ -> subModel, Cmd.batch [ ServerResponseViews.showSuccessToast "Lekce byla úspěšně zrušena."; Cmd.ofMsg Init ]
                | Error e -> subModel, e |> ServerResponseViews.showErrorToast
                |> mapFst (ActiveItemModel.Lesson >> Some >> (fun x -> { model with ActiveItemModel = x }))
            | DeleteLesson ->
                let subModel = { subModel with DeleteLessonForm = subModel.DeleteLessonForm
                                                             |> ValidatedForm.markAsSent
                                                             |> ValidatedForm.validateWith validateDeleteLesson }
                if subModel.DeleteLessonForm |> ValidatedForm.isValid then
                    { subModel with DeleteLessonForm = subModel.DeleteLessonForm |> ValidatedForm.startLoading },
                        Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.DeleteLesson)) subModel.DeleteLessonForm.FormData LessonDeleted
                else subModel, Cmd.none
                |> mapFst (ActiveItemModel.Lesson >> Some >> (fun x -> { model with ActiveItemModel = x }))
                |> mapSnd (Cmd.map (ActiveLessonMsg >> ActiveItemMsg))
            | LessonDeleted res ->
                let subModel = { subModel with DeleteLessonForm = subModel.DeleteLessonForm |> ValidatedForm.stopLoading }
                match res with
                | Ok _ -> subModel, Cmd.batch [ ServerResponseViews.showSuccessToast "Lekce byla úspěšně smazána."; Cmd.ofMsg Init ]
                | Error e -> subModel, e |> ServerResponseViews.showErrorToast
                |> mapFst (ActiveItemModel.Lesson >> Some >> (fun x -> { model with ActiveItemModel = x }))                
        | ActiveWorkshopMsg m, (Some (Workshop subModel)) ->
            match m with
            | DeleteWorkshop ->
                let subModel = { subModel with DeleteWorkshopForm = subModel.DeleteWorkshopForm
                                                             |> ValidatedForm.markAsSent
                                                             |> ValidatedForm.validateWith validateDeleteWorkshop }
                if subModel.DeleteWorkshopForm |> ValidatedForm.isValid then
                    { subModel with DeleteWorkshopForm = subModel.DeleteWorkshopForm |> ValidatedForm.startLoading },
                        Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.DeleteWorkshop)) subModel.DeleteWorkshopForm.FormData WorkshopDeleted
                else subModel, Cmd.none
                |> mapFst (ActiveItemModel.Workshop >> Some >> (fun x -> { model with ActiveItemModel = x }))
                |> mapSnd (Cmd.map (ActiveWorkshopMsg >> ActiveItemMsg))
            | WorkshopDeleted res ->
                let subModel = { subModel with DeleteWorkshopForm = subModel.DeleteWorkshopForm |> ValidatedForm.stopLoading }
                match res with
                | Ok _ -> subModel, Cmd.batch [ ServerResponseViews.showSuccessToast "Workshop byla úspěšně smazán."; Cmd.ofMsg Init ]
                | Error e -> subModel, e |> ServerResponseViews.showErrorToast
                |> mapFst (ActiveItemModel.Workshop >> Some >> (fun x -> { model with ActiveItemModel = x }))
        
        | _ -> model, Cmd.none                
