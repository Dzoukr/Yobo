module Yobo.Client.Login.Domain

open Yobo.Shared

type State = { Counter: Counter option }

type Msg =
    | Increment
    | Decrement
    | InitialCountLoaded of Result<Counter, exn>
    | Reset

