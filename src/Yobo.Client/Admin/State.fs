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
        | Ok users -> { state with Users = users }, Cmd.none
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
                    ExpirationDate = state.AddCreditsForm.ExpirationDate.Value } : Yobo.Shared.Admin.Domain.AddCredits)
                |> SecuredParam.create |> Cmd.ofAsyncResult adminAPI.AddCredits (FormSubmitted >> AddCreditsFormMsg)
        | FormSubmitted res ->
            match res with
            | Ok _ -> 
                { state
                    with AddCreditsForm = { state.AddCreditsForm with SelectedUserId = None }},
                        Toast.message "I am toast of type Info"
                        |> Toast.title "Info"
                        |> Toast.position Toast.TopCenter
                        |> Toast.success
            | Error e -> state, (e |> SharedView.serverErrorToToast)