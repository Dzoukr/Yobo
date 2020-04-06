module Yobo.Shared.Core.Reservations.Domain

open System

let minWeekOffset = -1 

module Queries =
    open Yobo.Shared.Core.Domain.Queries
    
    type Capacity =
        | LastFreeSpot
        | Free
        | Full
    
    type Availability =
        | Available of Capacity
        | Closed
        
    module Availability =
        let getAvailability startDate isCancelled capacity userReservations =
            let freeSpots = capacity - userReservations
            let isOpen = DateTimeOffset.UtcNow < startDate && (not isCancelled)
            match isOpen, freeSpots with
            | true, 1 -> LastFreeSpot |> Available
            | true, 0 -> Full |> Available 
            | true, _ -> Free |> Available
            | false, _ -> Closed
    
    type Lesson = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
        Availability : Availability
        UserReservation : LessonPayment option
        CancellableUntil : DateTimeOffset
        IsCancelled : bool
    }
    
    type OnlineLesson = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
        Availability : Availability
        UserReservation : LessonPayment option
        CancellableUntil : DateTimeOffset
        IsCancelled : bool
    }