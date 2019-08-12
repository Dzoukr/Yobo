module Yobo.Client.MyLessons.State

open System
open Yobo.Client.MyLessons.Domain
open Elmish
open Yobo.Client.Server
open Yobo.Client

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Init -> state, LoadMyLessons |> Cmd.ofMsg
    | LoadMyLessons ->
        state,
            ()
            |> SecuredParam.create
            |> Cmd.ofAsyncResult myLessonsAPI.GetMyLessons MyLessonsLoaded
    | MyLessonsLoaded res ->
        match res with
        | Ok lsns ->
           { state with Lessons = lsns }, Cmd.none
        | Error err -> state, (err |> SharedView.serverErrorToToast)