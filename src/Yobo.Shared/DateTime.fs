module Yobo.Shared.DateTime

open System

type DateTimeOffset with
    member x.StartOfTheDay () = DateTimeOffset(x.Year, x.Month, x.Day, 0, 0, 0, x.Offset)
    member x.EndOfTheDay () = x.Subtract(x.TimeOfDay) |> fun x -> x.Add(System.TimeSpan(0,23,59,59,999))