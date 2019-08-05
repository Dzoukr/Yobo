module Yobo.Client.Admin.Lessons.State

open System
open Yobo.Client.Admin.Lessons.Domain
open Elmish
open Yobo.Client.Http
open Thoth.Elmish
open Yobo.Client
open FSharp.Rop
open Yobo.Shared.Extensions

let private tryToTimeSpan (s:string) =
    let parts = s.Split([|':'|]) |> Array.toList
    match parts with
    | [h;m] ->
        match Int32.TryParse h, Int32.TryParse m with
        | (true, h), (true, m) ->
            TimeSpan(h, m, 0) |> Some
        | _ -> None
    | _ -> None

let private tryGetFromTo (s:string) (e:string) (d:DateTimeOffset) =
    match tryToTimeSpan s, tryToTimeSpan e with
    | Some st, Some en -> (d.Add(st), d.Add(en)) |> Some
    | _ -> None

let getValidLessonsToAdd (state:State) =
    state.SelectedDates
    |> List.map (fun x ->
        let st,en =
            x
            |> tryGetFromTo state.AddLessonForm.StartTime state.AddLessonForm.EndTime
            |> Option.defaultValue (DateTimeOffset.MinValue, DateTimeOffset.MinValue)
        ({
            Start = st
            End = en
            Name = state.AddLessonForm.Name
            Description = state.AddLessonForm.Description
        } : Yobo.Shared.Admin.Domain.AddLesson)
    )
    |> List.filter (fun x -> x.Start <> DateTimeOffset.MinValue)
    |> List.map Yobo.Shared.Admin.Validation.validateAddLesson
    |> Result.partition
    |> fst

let getValidWorkshopsToAdd (state:State) =
    state.SelectedDates
    |> List.map (fun x ->
        let st,en =
            x
            |> tryGetFromTo state.AddLessonForm.StartTime state.AddLessonForm.EndTime
            |> Option.defaultValue (DateTimeOffset.MinValue, DateTimeOffset.MinValue)
        ({
            Start = st
            End = en
            Name = state.AddLessonForm.Name
            Description = state.AddLessonForm.Description
        } : Yobo.Shared.Admin.Domain.AddWorkshop)
    )
    |> List.filter (fun x -> x.Start <> DateTimeOffset.MinValue)
    |> List.map Yobo.Shared.Admin.Validation.validateAddWorkshop
    |> Result.partition
    |> fst

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Init -> state, [ LoadLessons; LoadWorkshops] |> List.map Cmd.ofMsg |> Cmd.batch
    | LoadLessons ->
        state,
            state.WeekOffset
            |> Yobo.Shared.DateRange.getDateRangeForWeekOffset
            |> SecuredParam.create
            |> Cmd.ofAsyncResult adminAPI.GetLessonsForDateRange LessonsLoaded
    | LoadWorkshops ->
        state,
            state.WeekOffset
            |> Yobo.Shared.DateRange.getDateRangeForWeekOffset
            |> SecuredParam.create
            |> Cmd.ofAsyncResult adminAPI.GetWorkshopsForDateRange WorkshopsLoaded
    | LessonsLoaded res ->
        match res with
        | Ok less ->
            { state with Lessons = less}, Cmd.none
        | Error _ -> state, Cmd.none
    | WorkshopsLoaded res ->
        match res with
        | Ok v ->
            { state with Workshops = v}, Cmd.none
        | Error _ -> state, Cmd.none
    | WeekOffsetChanged o ->
        { state with WeekOffset = o }, [ LoadLessons; LoadWorkshops] |> List.map Cmd.ofMsg |> Cmd.batch
    | DateSelected d ->
        { state with SelectedDates = d :: state.SelectedDates }, Cmd.none
    | DateUnselected d ->
        let newDates = state.SelectedDates |> List.filter (fun x -> x <> d )
        let newCmd = if newDates.Length > 0 then Cmd.none else (AddLessonFormOpened(false) |> Cmd.ofMsg)
        { state with SelectedDates = newDates }, newCmd
    | AddLessonFormChanged f ->
        { state with AddLessonForm = f }, Cmd.none
    | AddLessonFormOpened o ->
        { state with FormOpened = o }, Cmd.none
    | SubmitAddLessonForm ->
        state,
            (state
            |> getValidLessonsToAdd
            |> SecuredParam.create
            |> Cmd.ofAsyncResult adminAPI.AddLessons (AddLessonFormSubmitted))
    | SubmitAddWorkshopForm ->
        state,
            (state
            |> getValidWorkshopsToAdd
            |> SecuredParam.create
            |> Cmd.ofAsyncResult adminAPI.AddWorkshops (AddLessonFormSubmitted))
    | AddLessonFormSubmitted res ->
        match res with
        | Ok _ -> { State.Init with WeekOffset = state.WeekOffset },
                    [
                        SharedView.successToast "Lekce úspěšně přidány."
                        LoadLessons |> Cmd.ofMsg
                        LoadWorkshops |> Cmd.ofMsg
                    ]
                    |> Cmd.batch
        | Error e -> state, (e |> SharedView.serverErrorToToast)
    | CancelLesson id ->
        state, (id |> SecuredParam.create |> Cmd.ofAsyncResult adminAPI.CancelLesson LessonCancelled)
    | DeleteWorkshop id ->
        state, (id |> SecuredParam.create |> Cmd.ofAsyncResult adminAPI.DeleteWorkshop WorkshopDeleted)
    | LessonCancelled res ->
        state, [ (res |> SharedView.resultToToast "Lekce byla úspěšně zrušena"); LoadLessons |> Cmd.ofMsg ] |> Cmd.batch
    | WorkshopDeleted res ->
        state, [ (res |> SharedView.resultToToast "Workshop byla úspěšně smazán"); LoadWorkshops |> Cmd.ofMsg ] |> Cmd.batch
                