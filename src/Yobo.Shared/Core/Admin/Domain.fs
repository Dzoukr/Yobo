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
    
    type LessonPayment =
        | Cash
        | Credits
    
    type Lesson = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
        Reservations : (User * LessonPayment) list
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
    
    type OnlineLesson = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
        Reservations : (User * LessonPayment) list
        IsCancelled : bool
        Capacity : int
    }