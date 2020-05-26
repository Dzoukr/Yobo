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
        Reservations : (User * Queries.LessonPayment) list
        IsCancelled : bool
        Capacity : int
    }
    
    type Workshop = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
    }