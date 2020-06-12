module Yobo.Shared.Core.Reservations.Domain

open System

let minWeekOffset = -1 

module Queries =
    open Yobo.Shared.Core.Domain.Queries
    
    type Lesson = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
        LessonStatus : LessonStatus
        ReservationStatus : ReservationStatus
    }
    
    type Workshop = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
    }