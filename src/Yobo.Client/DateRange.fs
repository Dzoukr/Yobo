module Yobo.Client.DateRange

open System
open Yobo.Shared.Extensions

let private closestMonday (date:DateTimeOffset) =
    let offset = date.DayOfWeek - DayOfWeek.Monday
    date.AddDays -(offset |> float) |> fun x -> x.StartOfTheDay()

let private closestSunday (date:DateTimeOffset) =
    let current = date.DayOfWeek |> int
    let offset = 7 - current
    date.AddDays (offset |> float) |> fun x -> x.EndOfTheDay()

let getWeekDateRange dayInWeek =
    (dayInWeek |> closestMonday), (dayInWeek |> closestSunday)

let getDateRangeForWeekOffset offset = DateTimeOffset.Now.AddDays(offset * 7 |> float) |> getWeekDateRange
    
let dateRangeToDays (startDate:DateTimeOffset,endDate:DateTimeOffset) =
    endDate.Subtract(startDate).TotalDays
    |> int
    |> (fun d -> [0..d])
    |> List.map (fun x ->
        startDate.AddDays (float x)
    )