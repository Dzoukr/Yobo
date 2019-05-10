module Yobo.Client.MyLessons.State

open System
open Yobo.Client.MyLessons.Domain
open Elmish
open Yobo.Client.Http
open Yobo.Client

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Init -> state, LoadMyLessons |> Cmd.ofMsg
    | LoadMyLessons ->
        state, Cmd.none
    | MyLessonsLoaded res ->
        match res with
        | Ok lsns ->
           { state with Lessons = lsns }, Cmd.none
        | Error err -> state, (err |> SharedView.serverErrorToToast)