module Yobo.Client.Admin.State

open Yobo.Client.Admin.Domain
open Elmish
open Yobo.Client.Http
open Thoth.Elmish
open Yobo.Client

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Init -> state, LoadUsers |> Cmd.ofMsg
    | LoadUsers -> state, (() |> SecuredParam.create |> Cmd.ofAsyncResult adminAPI.GetAllUsers UsersLoaded)
    | UsersLoaded res ->
        match res with
        | Ok users ->
            { state with Users = users }, Cmd.none
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
            { state with Lessons = { state.Lessons with WeekOffset = o } }, Cmd.none
        | DateSelected d ->
            { state with Lessons = { state.Lessons with SelectedDates = d :: state.Lessons.SelectedDates } }, Cmd.none
        | DateUnselected d ->
            { state with Lessons = { state.Lessons with SelectedDates = state.Lessons.SelectedDates |> List.filter (fun x -> x <> d ) } }, Cmd.none
        | StartChanged s ->
            { state with Lessons = { state.Lessons with StartTime = s } }, Cmd.none
        | EndChanged s ->
            { state with Lessons = { state.Lessons with EndTime = s } }, Cmd.none
        | NameChanged n ->
            { state with Lessons = { state.Lessons with Name = n } }, Cmd.none
        | DescriptionChanged n ->
            { state with Lessons = { state.Lessons with Description = n } }, Cmd.none