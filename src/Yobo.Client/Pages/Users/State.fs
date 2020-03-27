module Yobo.Client.Pages.Users.State

open System
open Domain
open Elmish
open Yobo.Client.Server
open Yobo.Client
open Yobo.Client.SharedView

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    match msg with
    | LoadUsers -> { model with UsersLoading = true }, Cmd.OfAsync.eitherAsResult (onUsersService (fun x -> x.GetAllUsers)) () UsersLoaded
    | UsersLoaded res ->
        let model = { model with UsersLoading = false }
        match res with
        | Ok users -> { model with Users = users }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast
//    | ToggleAddCreditsForm i ->
//        let userId = if state.SelectedUserId = Some i then None else Some i
//        { state with SelectedUserId = userId }, Cmd.none
//    | CalendarChanged date ->
//        { state with ExpirationDate = date }, Cmd.none
//    | CreditsChanged c -> { state with Credits = c }, Cmd.none
//    | SubmitForm ->
//        state,
//            ({  UserId = state.SelectedUserId.Value
//                Credits = state.Credits
//                Expiration = state.ExpirationDate.Value } : Yobo.Shared.Admin.Domain.AddCredits)
//            |> SecuredParam.create |> Cmd.ofAsyncResult adminAPI.AddCredits FormSubmitted
//    | FormSubmitted res ->
//        match res with
//        | Ok _ -> 
//            State.Init,
//                [
//                    SharedView.successToast "Kredity úspěšně přidány."
//                    LoadUsers |> Cmd.ofMsg ]
//                    |> Cmd.batch
//        | Error e -> state, (e |> SharedView.serverErrorToToast)