module Yobo.Core.Lessons.EventSerializer

open Yobo.Core

let toEvent = function
    | "Created", data -> data |> Serialization.objectFromJToken<CmdArgs.Create> |> Created
    | "ReservationAdded", data -> data |> Serialization.objectFromJToken<CmdArgs.AddReservation> |> ReservationAdded
    | n,_ -> failwithf "Unrecognized event %s" n

let toData = function
    | Created args -> "Created", (args |> Serialization.objectToJToken)
    | ReservationAdded args -> "ReservationAdded", (args |> Serialization.objectToJToken)