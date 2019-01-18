module Yobo.Shared.Admin.Domain

open System

type User = {
    Id : Guid
    Email : string
    FirstName : string
    LastName : string
    Activated : DateTimeOffset option
    Credits : int
    CreditsExpiration : DateTimeOffset option
}

type AddCredits = {
    UserId : Guid
    Credits : int
    Expiration : DateTimeOffset
}

type AddLesson = {
    Start : DateTimeOffset
    End : DateTimeOffset
    Name : string
    Description : string
}

type Lesson = {
    Id : Guid
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    Name : string
    Description : string
    Reservations : User list
}