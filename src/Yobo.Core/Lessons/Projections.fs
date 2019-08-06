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

type User = {
    UserId : Guid
    Credits : int
    CreditsExpiration : DateTimeOffset option
    CashReservationsBlockedUntil : DateTimeOffset option
    IsActivated : bool
}

type ExistingWorkshop = {
    Id : Guid
}