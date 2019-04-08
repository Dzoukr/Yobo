module Yobo.Core.Workshops.EventSerializer

open Yobo.Core

let toEvent = function
    | "Created", data -> data |> Serialization.objectFromJToken<CmdArgs.Create> |> Created
    | "Deleted", data -> data |> Serialization.objectFromJToken<CmdArgs.Delete> |> Deleted
    | n,_ -> failwithf "Unrecognized event %s" n

let toData = function
    | Created args -> "Created", (args |> Serialization.objectToJToken)
    | Deleted args -> "Deleted", (args |> Serialization.objectToJToken)

let streamPrefix = "Workshops-"

let (|WorkshopsEvent|_|) (event:CosmoStore.EventRead) =
    if event.StreamId.StartsWith(streamPrefix) then
        toEvent (event.Name, event.Data) |> Some
    else None