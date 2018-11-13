module Yobo.Client.Login.State

open Yobo.Client.Login.Domain
open Elmish
open Fable.PowerPack.Fetch
open Yobo.Shared
open Thoth.Json
open Fable.Import

let initialCounter = fetchAs<Counter> "/api/init" (Decode.Auto.generateDecoder<Counter>(true))

let urlUpdate (result: Option<Router.Page>) state =
    match result with
    | None ->
        Browser.console.error("Error parsing url: " + Browser.window.location.href)
        state, Router.modifyUrl state.Page
    | Some page ->
        let state = { state with Page = page }
        state, []

let init result =
    urlUpdate result State.Init

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Login -> { state with IsLogging = true }, Cmd.none
    | EmailChange v -> { state with Credentials = { state.Credentials with Email = v }}, Cmd.none
    | PasswordChange v -> { state with Credentials = { state.Credentials with Password = v }}, Cmd.none

    // match currentModel.Counter, msg with
    // | Some x, Increment ->
    //     let nextModel = { currentModel with Counter = Some { x with Value = x.Value + 1 } }
    //     nextModel, Cmd.none
    // | Some x, Decrement ->
    //     let nextModel = { currentModel with Counter = Some { x with Value = x.Value - 1 } }
    //     nextModel, Cmd.none
    // | _, InitialCountLoaded (Ok initialCount)->
    //     let nextModel = { Counter = Some initialCount }
    //     nextModel, Cmd.none
    // | _, Reset ->
    //     currentModel, Cmd.ofPromise
    //                     initialCounter
    //                     []
    //                     (Ok >> InitialCountLoaded)
    //                     (Error >> InitialCountLoaded)
    // | _ -> currentModel, Cmd.none

