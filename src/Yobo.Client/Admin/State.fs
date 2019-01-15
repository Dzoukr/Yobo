module Yobo.Client.Admin.State

open System
open Yobo.Client.Admin.Domain
open Elmish
open Yobo.Client.Http
open Thoth.Elmish
open Yobo.Client
open FSharp.Rop

let tryToTimeSpan (s:string) =
    let parts = s.Split([|':'|]) |> Array.toList
    match parts with
    | [h;m] ->
        match Int32.TryParse h, Int32.TryParse m with
        | (true, h), (true, m) ->
            TimeSpan(h, m, 0) |> Some
        | _ -> None
    | _ -> None

let tryGetFromTo (s:string) (e:string) (d:DateTime) =
    match tryToTimeSpan s, tryToTimeSpan e with
    | Some st, Some en -> (d.Date.Add(st), d.Date.Add(en)) |> Some
    | _ -> None

let getValidLessonsToAdd (state:AddLessonsForm) =
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
    | Init -> state, LoadUsers |> Cmd.ofMsg
    | LoadUsers -> state, (() |> SecuredParam.create |> Cmd.ofAsyncResult adminAPI.GetAllUsers UsersLoaded)
    | UsersLoaded res ->
        match res with
        | Ok users ->
            { state with Users = users }, Cmd.none
        | Error _ -> state, Cmd.none
    | LoadLessons -> state, (() |> SecuredParam.create |> Cmd.ofAsyncResult adminAPI.GetAllLessons LessonsLoaded)
    | LessonsLoaded res ->
        match res with
        | Ok less ->
            { state with Lessons = less}, Cmd.none
        | Error _ -> state, Cmd.none

    | ToggleAddCreditsForm i ->
        let userId = if state.AddCreditsForm.SelectedUserId = Some i then None else Some i
        { state with AddCreditsForm = { state.AddCreditsForm with SelectedUserId = userId } }, Cmd.none
    | AddCreditsFormMsg m ->
        match m with
        | CalendarChanged date ->
            { state with AddCreditsForm = { state.AddCreditsForm with ExpirationDate = date } }, Cmd.none
        | CreditsChanged c -> { state with AddCreditsForm = { state.AddCreditsForm with Credits = c } }, Cmd.none
        | SubmitForm ->
            state,
                ({  UserId = state.AddCreditsForm.SelectedUserId.Value
                    Credits = state.AddCreditsForm.Credits
                    ExpirationUtc = state.AddCreditsForm.ExpirationDate.Value } : Yobo.Shared.Admin.Domain.AddCredits)
                |> SecuredParam.create |> Cmd.ofAsyncResult adminAPI.AddCredits (FormSubmitted >> AddCreditsFormMsg)
        | FormSubmitted res ->
            match res with
            | Ok _ -> 
                { state
                    with AddCreditsForm = AddCreditsForm.Init }, [
                        SharedView.successToast "Kredity úspěšně přidány."
                        LoadUsers |> Cmd.ofMsg ]
                        |> Cmd.batch
            | Error e -> state, (e |> SharedView.serverErrorToToast)
    | LessonsMsg m ->
        match m with
        | WeekOffsetChanged o ->
            { state with AddLessonsForm = { state.AddLessonsForm with WeekOffset = o } }, Cmd.none
        | DateSelected d ->
            { state with AddLessonsForm = { state.AddLessonsForm with SelectedDates = d :: state.AddLessonsForm.SelectedDates } }, Cmd.none
        | DateUnselected d ->
            let newDates = state.AddLessonsForm.SelectedDates |> List.filter (fun x -> x <> d )
            let newCmd = if newDates.Length > 0 then Cmd.none else (FormOpened(false) |> LessonsMsg |> Cmd.ofMsg)
            { state with AddLessonsForm = { state.AddLessonsForm with SelectedDates = newDates } }, newCmd
        | StartChanged s ->
            { state with AddLessonsForm = { state.AddLessonsForm with StartTime = s } }, Cmd.none
        | EndChanged s ->
            { state with AddLessonsForm = { state.AddLessonsForm with EndTime = s } }, Cmd.none
        | NameChanged n ->
            { state with AddLessonsForm = { state.AddLessonsForm with Name = n } }, Cmd.none
        | DescriptionChanged n ->
            { state with AddLessonsForm = { state.AddLessonsForm with Description = n } }, Cmd.none
        | FormOpened o ->
            { state with AddLessonsForm = { state.AddLessonsForm with FormOpened = o } }, Cmd.none
        | SubmitLessonsForm ->
            state,
                (state.AddLessonsForm
                |> getValidLessonsToAdd
                |> SecuredParam.create
                |> Cmd.ofAsyncResult adminAPI.AddLessons (LessonsFormSubmitted >> LessonsMsg))
        | LessonsFormSubmitted res ->
            match res with
            | Ok _ -> 
                { state
                    with AddLessonsForm = AddLessonsForm.Init }, [
                        SharedView.successToast "Lekce úspěšně přidány."
                        LoadUsers |> Cmd.ofMsg ]
                        |> Cmd.batch
            | Error e -> state, (e |> SharedView.serverErrorToToast)
                