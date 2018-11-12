module Yobo.Client.Login.State

open Yobo.Client.Login.Domain
open Elmish
open Fable.PowerPack.Fetch
open Yobo.Shared
open Thoth.Json

let initialCounter = fetchAs<Counter> "/api/init" (Decode.Auto.generateDecoder<Counter>(true))

let update (msg : Msg) (currentModel : State) : State * Cmd<Msg> =
    match currentModel.Counter, msg with
    | Some x, Increment ->
        let nextModel = { currentModel with Counter = Some { x with Value = x.Value + 1 } }
        nextModel, Cmd.none
    | Some x, Decrement ->
        let nextModel = { currentModel with Counter = Some { x with Value = x.Value - 1 } }
        nextModel, Cmd.none
    | _, InitialCountLoaded (Ok initialCount)->
        let nextModel = { Counter = Some initialCount }
        nextModel, Cmd.none
    | _, Reset ->
        currentModel, Cmd.ofPromise
                        initialCounter
                        []
                        (Ok >> InitialCountLoaded)
                        (Error >> InitialCountLoaded)
    | _ -> currentModel, Cmd.none

let init () =
    { Counter = Some { Value = 123; Message = "TOTO JE LOGIN"}}, Cmd.none