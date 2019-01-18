module Yobo.Shared.Extensions

type System.DateTimeOffset with
    member x.StartOfTheDay () = x.Subtract(x.TimeOfDay)

type System.DateTimeOffset with
    member x.EndOfTheDay () = x.Subtract(x.TimeOfDay) |> fun x -> x.Add(System.TimeSpan(0,23,59,59,999))