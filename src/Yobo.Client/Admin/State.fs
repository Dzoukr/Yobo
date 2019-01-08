module Yobo.Client.Admin.State

open Yobo.Client.Admin.Domain
open Elmish
open Yobo.Client.Http

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
    | CalendarChanged date ->
        { state with AddCreditsForm = { state.AddCreditsForm with ExpirationDate = date } }, Cmd.none