module Yobo.Shared.DateTime

open System

[<RequireQualifiedAccess>]
module DateTimeOffset =
    let startOfTheDay (x:DateTimeOffset) = DateTimeOffset(x.Year, x.Month, x.Day, 0, 0, 0, x.Offset)
    let endOfTheDay (x:DateTimeOffset) = x.Subtract(x.TimeOfDay) |> fun x -> x.Add(System.TimeSpan(0,23,59,59,999))
    let toCzDate (date:DateTimeOffset) = date.ToString("dd. MM. yyyy")
    let toCzTime (date:DateTimeOffset) = date.ToString("HH:mm")
    let toCzDateTime (date:DateTimeOffset) = date.ToString("dd. MM. yyyy HH:mm")
    
let private closestMonday (date:DateTimeOffset) =
    let offset = (date.DayOfWeek - DayOfWeek.Monday) |> float
    let offset = if offset < 0. then 6. else offset
    date.AddDays (-offset) |> DateTimeOffset.startOfTheDay

let private closestSunday (date:DateTimeOffset) =
    date 
    |> closestMonday
    |> fun x -> x.AddDays 6.
    |> DateTimeOffset.endOfTheDay

[<RequireQualifiedAccess>]
module DateRange =
    let getWeekDateRange dayInWeek =
        (dayInWeek |> closestMonday), (dayInWeek |> closestSunday)

    let getDateRangeForWeekOffset offset = DateTimeOffset.Now.AddDays(offset * 7 |> float) |> getWeekDateRange
    
    let dateRangeToDays (startDate:DateTimeOffset,endDate:DateTimeOffset) =
        endDate.Subtract(startDate).TotalDays
        |> int
        |> (fun d -> [0..d])
        |> List.map (fun x -> startDate.AddDays (float x))
        |> List.map (fun x -> DateTimeOffset(DateTime(x.Year, x.Month, x.Day, 0, 0, 0)))

[<RequireQualifiedAccess>]
module DayOfWeek =        
    let toCzDay = function
        | DayOfWeek.Monday -> "Pondělí"
        | DayOfWeek.Tuesday -> "Úterý"
        | DayOfWeek.Wednesday -> "Středa"
        | DayOfWeek.Thursday -> "Čtvrtek"
        | DayOfWeek.Friday -> "Pátek"
        | DayOfWeek.Saturday -> "Sobota"
        | DayOfWeek.Sunday -> "Neděle"
        | _ -> "Osmý den schází nám"