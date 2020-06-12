module Yobo.Libraries.DateTime

open System

[<RequireQualifiedAccess>]
module DateTimeOffset =
    let toCzDateTimeOffset (dt:DateTimeOffset) =
        let tz = System.TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")
        dt.ToOffset(tz.GetUtcOffset(dt))
        
    let withHoursMins (hrs,mins) (dt:DateTimeOffset) =
        DateTimeOffset(dt.Year, dt.Month, dt.Day, hrs, mins, 0, dt.Offset)