module Yobo.Core.Lessons.EventSerializer

open Yobo.Core

let toEvent = function
    | "Created", data -> data |> Serialization.objectFromJToken<CmdArgs.Create> |> Created
    | n,_ -> failwithf "Unrecognized event %s" n

let toData = function
    | Created args -> "Created", (args |> Serialization.objectToJToken)