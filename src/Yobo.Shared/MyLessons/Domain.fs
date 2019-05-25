module Yobo.Shared.MyLessons.Domain

open System

type Lesson = {
    Id : Guid
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    Name : string
    Description : string
    CreditsUsed : bool
}