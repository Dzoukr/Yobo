module Yobo.Core.Lessons.Projections

open System

type UserReservation = {
    UserId : Guid
    Count : int
    UseCredits : bool
}

type ExistingLesson = {
    Id : Guid
    Reservations : UserReservation list
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    IsCancelled : bool
}