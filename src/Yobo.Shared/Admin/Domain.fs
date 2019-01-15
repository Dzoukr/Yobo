module Yobo.Shared.Admin.Domain

open System

type User = {
    Id : Guid
    Email : string
    FirstName : string
    LastName : string
    ActivatedUtc : DateTime option
    Credits : int
    CreditsExpirationUtc : DateTime option
}

type AddCredits = {
    UserId : Guid
    Credits : int
    ExpirationUtc : DateTime
}

type AddLesson = {
    Start : DateTime
    End : DateTime
    Name : string
    Description : string
}

type Lesson = {
    Start : DateTime
    End : DateTime
    Name : string
    Description : string
    Reservations : User list
}