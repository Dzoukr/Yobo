module Yobo.Shared.Core.Admin.Domain

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
    
    type LessonReservation =
        | Cash
        | Credits
    
    type Lesson = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
        Reservations : (User * LessonReservation) list
        IsCancelled : bool
        Capacity : int
    }