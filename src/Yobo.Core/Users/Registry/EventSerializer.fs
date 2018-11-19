module Yobo.Core.Users.Registry.EventSerializer

open Yobo.Core

let toEvent = function
    | "Added", data -> data |> Serialization.objectFromJToken<CmdArgs.Add> |> Added
    | "Removed", data -> data |> Serialization.objectFromJToken<CmdArgs.Remove> |> Removed
    | n,_ -> failwithf "Unrecognized event %s" n

let toData = function
    | Added args -> "Added", (args |> Serialization.objectToJToken)
    | Removed args -> "Removed", (args |> Serialization.objectToJToken)