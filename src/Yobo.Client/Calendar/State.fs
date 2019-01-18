module Yobo.Client.Calendar.State

open System
open Yobo.Client.Calendar.Domain
open Elmish
open Yobo.Client.Http
open Yobo.Client

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Init -> state, LoadUserLessons |> Cmd.ofMsg
    | LoadUserLessons -> state, (state.WeekOffset |> DateRange.getDateRangeForWeekOffset |> SecuredParam.create |> Cmd.ofAsyncResult calendarAPI.GetLessonsForDateRange UserLessonsLoaded)
    | UserLessonsLoaded res ->
        match res with
        | Ok users ->
           state, Cmd.none
        | Error _ -> state, Cmd.none
    | WeekOffsetChanged o -> { state with WeekOffset = o }, LoadUserLessons |> Cmd.ofMsg