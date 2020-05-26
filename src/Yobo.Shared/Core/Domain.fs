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

let getLessonCancellingDate (d:DateTimeOffset) =
    d
    |> DateTimeOffset.startOfTheDay
    |> (fun x -> x.AddHours 10.)

module Queries =
    type LessonPayment =
        | Cash
        | Credits
    
    module LessonPayment =        
        let fromUseCredits uc = if uc then Credits else Cash
        
    type Capacity =
        | LastFreeSpot
        | Free
    
    type UnavailabilityReason =
        | Full
        | AlreadyStarted
        | Cancelled
    
    type UserReservation = {
        Payment : LessonPayment
        IsCancellable : bool
    }
    
    type LessonAvailability =
        | Available of Capacity
        | Unavailable of UnavailabilityReason
    
    type ReservationAvailability =
        | AlreadyReserved of LessonPayment * canBeCancelled:bool
        | Reservable of LessonPayment
        | Unreservable
