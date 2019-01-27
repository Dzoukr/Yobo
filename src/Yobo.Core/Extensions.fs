module Yobo.Core.Extensions

open System

type DateTimeOffset with
    member x.ToCzDateTimeOffset () =
        let tz = System.TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")
        x.ToOffset(tz.GetUtcOffset(x))