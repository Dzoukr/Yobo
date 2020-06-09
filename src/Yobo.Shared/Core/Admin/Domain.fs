module Yobo.Shared.Core.Admin.Domain

open System
open Yobo.Shared.Core.Domain

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
        
    type Lesson = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
        Reservations : (User * LessonPayment) list
        IsCancelled : bool
        Capacity : int
        CancellableBeforeStart : TimeSpan
    }
    
    type Workshop = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
    }