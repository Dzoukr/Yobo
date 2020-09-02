module Yobo.Shared.Core.Domain

open System
open Yobo.Shared.DateTime

let calculateCredits amount expiration =
    match expiration with
    | None -> amount
    | Some exp ->
        if DateTimeOffset.UtcNow > exp then 0 else amount
        
let canLessonBeCancelled (isCanceled:bool) (lessonStart:DateTimeOffset) =
    (not isCanceled) && lessonStart > DateTimeOffset.Now
    
let canLessonBeDeleted (lessonStart:DateTimeOffset) =
    lessonStart > DateTimeOffset.Now

type LessonPayment =
    | Cash
    | Credits

module LessonPayment =        
    let fromUseCredits uc = if uc then Credits else Cash
    let toUseCredits = function
        | Cash -> false
        | Credits -> true

module Queries =
    type Capacity =
        | LastFreeSpot
        | Free
    
    type ClosedReason =
        | Full
        | AlreadyStarted
        | Cancelled
    
    type LessonStatus =
        | Open of Capacity
        | Closed of ClosedReason
        
    type ReservationStatus =
        | CanBeReserved of LessonPayment
        | Reserved of LessonPayment * canBeCancelled:bool
        | Unreservable 
    
    module LessonStatus =
        let getLessonStatus startDate isCancelled capacity allReservationCount =
            let freeSpots = capacity - allReservationCount
            let isAlreadyStarted = DateTimeOffset.UtcNow > startDate
            
            if isCancelled then Cancelled |> Closed
            else if isAlreadyStarted then AlreadyStarted |> Closed
            else
                match freeSpots with
                | 0 -> Full |> Closed
                | 1 -> LastFreeSpot |> Open
                | _ -> Free |> Open 
    
    module ReservationStatus =
        let getReservationStatus (lessonStatus:LessonStatus) (startDate:DateTimeOffset) (cancelLimit:TimeSpan) (userReservation:LessonPayment option) userCredits userCreditsExpiration (cashBlockedUntil:DateTimeOffset option) =
            let canBeCancelled = DateTimeOffset.UtcNow <= startDate.Subtract(cancelLimit)
            match lessonStatus, userReservation with
            | Closed _, None -> Unreservable
            | Closed Full, Some res -> Reserved(res, canBeCancelled)
            | Closed _, Some res -> Reserved(res, false)
            | Open _, Some res -> Reserved(res, canBeCancelled)
            | Open _, None ->
                let canUseCredits =
                    match userCredits, userCreditsExpiration with
                    | credits, Some expiry -> credits > 0 && expiry > startDate
                    | _ -> false
                let canUseCash =
                    match cashBlockedUntil with
                    | None -> true
                    | Some until -> until < DateTimeOffset.UtcNow
                match canUseCredits, canUseCash with
                | true, _ -> CanBeReserved(Credits)
                | false, true -> CanBeReserved(Cash)
                | false, false -> Unreservable
        