module Yobo.Core.Lessons.EventSerializer

open Yobo.Core

let toEvent = function
    | "Created", data -> data |> Serialization.objectFromJToken<CmdArgs.Create> |> Created
    | "ReservationAdded", data -> data |> Serialization.objectFromJToken<CmdArgs.AddReservation> |> ReservationAdded
    | "ReservationCancelled", data -> data |> Serialization.objectFromJToken<CmdArgs.CancelReservation> |> ReservationCancelled
    | n,_ -> failwithf "Unrecognized event %s" n

let toData = function
    | Created args -> "Created", (args |> Serialization.objectToJToken)
    | ReservationAdded args -> "ReservationAdded", (args |> Serialization.objectToJToken)
    | ReservationCancelled args -> "ReservationCancelled", (args |> Serialization.objectToJToken)

let streamPrefix = "Lessons-"

let (|LessonsEvent|_|) (event:CosmoStore.EventRead) =
    if event.StreamId.StartsWith(streamPrefix) then
        toEvent (event.Name, event.Data) |> Some
    else None