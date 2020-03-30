module Yobo.Shared.DateTime

open System

[<RequireQualifiedAccess>]
module DateTimeOffset =
    let startOfTheDay (x:DateTimeOffset) = DateTimeOffset(x.Year, x.Month, x.Day, 0, 0, 0, x.Offset)
    let endOfTheDay (x:DateTimeOffset) = x.Subtract(x.TimeOfDay) |> fun x -> x.Add(System.TimeSpan(0,23,59,59,999))
    let toCzDate (date:DateTimeOffset) = date.ToString("dd. MM. yyyy")
    let toCzTime (date:DateTimeOffset) = date.ToString("HH:mm")
    let toCzDateTime (date:DateTimeOffset) = date.ToString("dd. MM. yyyy HH:mm")