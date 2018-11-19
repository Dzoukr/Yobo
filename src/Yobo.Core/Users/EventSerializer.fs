module Yobo.Core.Users.EventSerializer

open Yobo.Core

let toEvent = function
    | "Registered", data -> data |> Serialization.objectFromJToken<CmdArgs.Register> |> Registered
    | n,_ -> failwithf "Unrecognized event %s" n

let toData = function
    | Registered args -> "Registered", (args |> Serialization.objectToJToken)