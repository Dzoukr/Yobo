module Yobo.Client.Calendar.State

open System
open Yobo.Client.Calendar.Domain
open Elmish
open Yobo.Client.Http
open Yobo.Client

let private innerUpdate (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Init -> state, LoadUserLessons |> Cmd.ofMsg
    | LoadUserLessons -> state, (state.WeekOffset |> DateRange.getDateRangeForWeekOffset |> SecuredParam.create |> Cmd.ofAsyncResult calendarAPI.GetLessonsForDateRange UserLessonsLoaded)
    | UserLessonsLoaded res ->
        match res with
        | Ok lsns ->
           { state with Lessons = lsns }, Cmd.none
        | Error err -> state, (err |> SharedView.serverErrorToToast)
    | WeekOffsetChanged o -> { state with WeekOffset = o }, LoadUserLessons |> Cmd.ofMsg
    | AddReservation args -> state, (args |> SecuredParam.create |> Cmd.ofAsyncResult calendarAPI.AddReservation ReservationAdded)
    | ReservationAdded res ->
        state, [ (res |> SharedView.resultToToast "Lekce byla úspěšně zarezervována"); LoadUserLessons |> Cmd.ofMsg ] |> Cmd.batch
    | CancelReservation args -> state, (args |> SecuredParam.create |> Cmd.ofAsyncResult calendarAPI.CancelReservation ReservationCancelled)
    | ReservationCancelled res ->
        state, [ (res |> SharedView.resultToToast "Rezervace byla úspěšně zrušena"); LoadUserLessons |> Cmd.ofMsg ] |> Cmd.batch

let update (msg : Msg) (state : State) : State * Cmd<Msg> * bool =
    let state, cmd = innerUpdate msg state
    match msg with
    | ReservationAdded _ | ReservationCancelled _ -> state, cmd, true
    | _ -> state, cmd, false