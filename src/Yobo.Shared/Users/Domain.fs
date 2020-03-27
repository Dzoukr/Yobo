module Yobo.Shared.Users.Domain

open System

module Queries =
    type User = {
        Id : Guid
        Email : string
        FirstName : string
        LastName : string
        Activated : DateTimeOffset option
        Credits : int
        CreditsExpiration : DateTimeOffset option
        CashReservationBlockedUntil : DateTimeOffset option
    }