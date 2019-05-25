module Yobo.Shared.Admin.Domain

open System
open Yobo.Shared.Domain

type Lesson = {
    Id : Guid
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    Name : string
    Description : string
    Reservations : (User * UserReservation) list
    IsCancelled : bool
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

type AddWorkshop = {
    Start : DateTimeOffset
    End : DateTimeOffset
    Name : string
    Description : string
}