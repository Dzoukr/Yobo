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
    | AddCredit -> state, Cmd.none
    | ToggleAddCreditsForm i ->
        let formId = if state.AddCreditsOpenedForm = Some i then None else Some i
        { state with AddCreditsOpenedForm = formId }, Cmd.none
    | CalendarChanged date ->
        Fable.Import.Browser.console.log(date)
        state, Cmd.none