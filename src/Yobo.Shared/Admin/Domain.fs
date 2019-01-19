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

type Payment =
    | Cash
    | Credits

type UserReservation =
    | ForOne of Payment
    | ForTwo
with
    member x.ToIntAndBool =
        match x with
        | ForOne (Cash) -> 1, false
        | ForOne (Credits) -> 1, true
        | ForTwo -> 2, true

type Lesson = {
    Id : Guid
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    Name : string
    Description : string
    Reservations : (User * UserReservation) list
}