module Yobo.Client.Admin.Lessons.State

open System
open Yobo.Client.Admin.Lessons.Domain
open Elmish
open Yobo.Client.Http
open Thoth.Elmish
open Yobo.Client
open FSharp.Rop

let private tryToTimeSpan (s:string) =
    let parts = s.Split([|':'|]) |> Array.toList
    match parts with
    | [h;m] ->
        match Int32.TryParse h, Int32.TryParse m with
        | (true, h), (true, m) ->
            TimeSpan(h, m, 0) |> Some
        | _ -> None
    | _ -> None

let private tryGetFromTo (s:string) (e:string) (d:DateTime) =
    match tryToTimeSpan s, tryToTimeSpan e with
    | Some st, Some en -> (d.Date.Add(st), d.Date.Add(en)) |> Some
    | _ -> None

let getValidLessonsToAdd (state:State) =
    state.SelectedDates
    |> List.map (fun x ->
        let st,en =
            x
            |> tryGetFromTo state.StartTime state.EndTime
            |> Option.defaultValue (DateTime.MinValue, DateTime.MinValue)
        ({
            Start = st
            End = en
            Name = state.Name
            Description = state.Description
        } : Yobo.Shared.Admin.Domain.AddLesson)
    )
    |> List.filter (fun x -> x.Start <> DateTime.MinValue)
    |> List.map Yobo.Shared.Admin.Validation.validateAddLesson
    |> Result.partition
    |> fst

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Init -> state, LoadLessons |> Cmd.ofMsg
    | LoadLessons -> state, (() |> SecuredParam.create |> Cmd.ofAsyncResult adminAPI.GetAllLessons LessonsLoaded)
    | LessonsLoaded res ->
        match res with
        | Ok less ->
            { state with Lessons = less}, Cmd.none
        | Error _ -> state, Cmd.none
    | WeekOffsetChanged o ->
        { state with WeekOffset = o }, Cmd.none
    | DateSelected d ->
        { state with SelectedDates = d :: state.SelectedDates }, Cmd.none
    | DateUnselected d ->
        let newDates = state.SelectedDates |> List.filter (fun x -> x <> d )
        let newCmd = if newDates.Length > 0 then Cmd.none else (FormOpened(false) |> Cmd.ofMsg)
        { state with SelectedDates = newDates }, newCmd
    | StartChanged s ->
        { state with StartTime = s }, Cmd.none
    | EndChanged s ->
        { state with EndTime = s }, Cmd.none
    | NameChanged n ->
        { state with Name = n }, Cmd.none
    | DescriptionChanged n ->
        { state with Description = n }, Cmd.none
    | FormOpened o ->
        { state with FormOpened = o }, Cmd.none
    | SubmitLessonsForm ->
        state,
            (state
            |> getValidLessonsToAdd
            |> SecuredParam.create
            |> Cmd.ofAsyncResult adminAPI.AddLessons (LessonsFormSubmitted))
    | LessonsFormSubmitted res ->
        match res with
        | Ok _ -> State.Init,
                     [
                            SharedView.successToast "Lekce úspěšně přidány."
                            LoadLessons |> Cmd.ofMsg ]
                     |> Cmd.batch
        | Error e -> state, (e |> SharedView.serverErrorToToast)
                