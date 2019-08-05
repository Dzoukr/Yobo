module Yobo.Shared.DateRange

open System
open Yobo.Shared.Extensions

let private closestMonday (date:DateTimeOffset) =
    let offset = (date.DayOfWeek - DayOfWeek.Monday) |> float
    let offset = if offset < 0. then 6. else offset
    date.AddDays (-offset) |> fun x -> x.StartOfTheDay()

let private closestSunday (date:DateTimeOffset) =
    date 
    |> closestMonday
    |> fun x -> x.AddDays 6.
    |> fun x -> x.EndOfTheDay()
    
let getWeekDateRange dayInWeek =
    (dayInWeek |> closestMonday), (dayInWeek |> closestSunday)

let getDateRangeForWeekOffset offset = DateTimeOffset.Now.AddDays(offset * 7 |> float) |> getWeekDateRange
    
let dateRangeToDays (startDate:DateTimeOffset,endDate:DateTimeOffset) =
    endDate.Subtract(startDate).TotalDays
    |> int
    |> (fun d -> [0..d])
    |> List.map (fun x -> startDate.AddDays (float x))
    |> List.map (fun x -> DateTimeOffset(DateTime(x.Year, x.Month, x.Day, 0, 0, 0)))
    