module Yobo.Client.Admin.Users.State

open System
open Yobo.Client.Admin.Users.Domain
open Elmish
open Yobo.Client.Http
open Yobo.Client

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Init -> state, LoadUsers |> Cmd.ofMsg
    | LoadUsers -> { state with UsersLoading = true }, (() |> SecuredParam.create |> Cmd.ofAsyncResult adminAPI.GetAllUsers UsersLoaded)
    | UsersLoaded res ->
        match res with
        | Ok users ->
            { state with Users = users; UsersLoading = false }, Cmd.none
        | Error _ -> { state with UsersLoading = false }, Cmd.none
    | ToggleAddCreditsForm i ->
        let userId = if state.SelectedUserId = Some i then None else Some i
        { state with SelectedUserId = userId }, Cmd.none
    | CalendarChanged date ->
        { state with ExpirationDate = date }, Cmd.none
    | CreditsChanged c -> { state with Credits = c }, Cmd.none
    | SubmitForm ->
        state,
            ({  UserId = state.SelectedUserId.Value
                Credits = state.Credits
                ExpirationUtc = state.ExpirationDate.Value } : Yobo.Shared.Admin.Domain.AddCredits)
            |> SecuredParam.create |> Cmd.ofAsyncResult adminAPI.AddCredits FormSubmitted
    | FormSubmitted res ->
        match res with
        | Ok _ -> 
            State.Init,
                [
                    SharedView.successToast "Kredity úspěšně přidány."
                    LoadUsers |> Cmd.ofMsg ]
                    |> Cmd.batch
        | Error e -> state, (e |> SharedView.serverErrorToToast)
    