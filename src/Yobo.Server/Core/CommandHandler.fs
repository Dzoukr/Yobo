module Yobo.Server.Core.CommandHandler

open System
open Domain
open Yobo.Shared.Core.Domain
open Yobo.Shared.Core.Domain.Queries
open Yobo.Shared.Errors
open FSharp.Rop.Result.Operators
open Yobo.Server.Core.Domain.CmdArgs

module Projections =
    type ExistingUser = {
        Id : Guid
        IsActivated : bool
        Credits : int
        CreditsExpiration : DateTimeOffset option
        CashReservationBlockedUntil : DateTimeOffset option
    }
    
    type UserReservation = {
        UserId : Guid
        CreditsExpiration : DateTimeOffset option
        UseCredits : bool
    }

    type ExistingLesson = {
        Id : Guid
        Reservations : UserReservation list
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        IsCancelled : bool
        Capacity : int
        CancellableBeforeStart : TimeSpan
    }
    
    type ExistingWorkshop = {
        Id : Guid
    }

let private onlyIfActivated (user:Projections.ExistingUser) =
    if user.IsActivated then Ok user else DomainError.UserNotActivated |> Error

let private onlyIfCanBeCancelled (lsn:Projections.ExistingLesson) =
    if Yobo.Shared.Core.Domain.canLessonBeCancelled lsn.IsCancelled lsn.StartDate then Ok lsn
    else Error DomainError.LessonCannotBeCancelled

let private onlyIfCanBeDeleted (lsn:Projections.ExistingLesson) =
    if Yobo.Shared.Core.Domain.canLessonBeDeleted lsn.StartDate then Ok lsn
    else Error DomainError.LessonCannotBeCancelled

let addCredits (user:Projections.ExistingUser) (args:CmdArgs.AddCredits) =
    user
    |> onlyIfActivated
    |>> (fun _ -> CreditsAdded args)
    |>> List.singleton
    
let setExpiration (user:Projections.ExistingUser) (args:CmdArgs.SetExpiration) =
    user
    |> onlyIfActivated
    |>> (fun _ -> ExpirationSet args)
    |>> List.singleton
    
let createLesson (args:CmdArgs.CreateLesson) =
    [ LessonCreated args ] |> Ok
    
let createWorkshop (args:CmdArgs.CreateWorkshop) =
    [ WorkshopCreated args ] |> Ok            
    
let changeLessonDescription (lesson:Projections.ExistingLesson) (args:CmdArgs.ChangeLessonDescription) =
    [ LessonDescriptionChanged args ] |> Ok

let cancelLesson (lesson:Projections.ExistingLesson) (args:CmdArgs.CancelLesson) =
    lesson
    |> onlyIfCanBeCancelled
    |>> (fun lsn ->
        let resCancels = lsn.Reservations |> List.map ((fun x -> { LessonId = lsn.Id; UserId = x.UserId } : CmdArgs.CancelLessonReservation) >> LessonReservationCancelled) 
        let refund,unblock = lsn.Reservations |> List.partition (fun x -> x.UseCredits)
        let refunds =
            refund
            |> List.map ((fun x -> { UserId = x.UserId } : CmdArgs.RefundCredit ) >> CreditRefunded)
        let unblocks =
            unblock
            |> List.map ((fun x -> { UserId = x.UserId } : CmdArgs.UnblockCashReservations) >> CashReservationsUnblocked)
        let extends =
            refund
            |> List.filter (fun x -> x.CreditsExpiration.IsSome)
            |> List.map (fun x -> { UserId = x.UserId; Expiration = x.CreditsExpiration.Value.Add(TimeSpan.FromDays 7.) } : CmdArgs.SetExpiration)
            |> List.map ExpirationSet
        [
            yield! resCancels
            yield! extends
            yield! refunds
            yield! unblocks
            yield LessonCancelled args
        ]
    )
    
let deleteLesson (lesson:Projections.ExistingLesson) (args:CmdArgs.DeleteLesson) =
    lesson
    |> onlyIfCanBeDeleted
    |>> (fun lsn ->
        let resCancels = lsn.Reservations |> List.map ((fun x -> { LessonId = lsn.Id; UserId = x.UserId } : CmdArgs.CancelLessonReservation) >> LessonReservationCancelled) 
        let refund,unblock = lsn.Reservations |> List.partition (fun x -> x.UseCredits)
        let refunds =
            refund
            |> List.map ((fun x -> { UserId = x.UserId } : CmdArgs.RefundCredit ) >> CreditRefunded)
        let unblocks =
            unblock
            |> List.map ((fun x -> { UserId = x.UserId } : CmdArgs.UnblockCashReservations) >> CashReservationsUnblocked)
        let extends =
            refund
            |> List.filter (fun x -> x.CreditsExpiration.IsSome)
            |> List.map (fun x -> { UserId = x.UserId; Expiration = x.CreditsExpiration.Value.Add(TimeSpan.FromDays 7.) } : CmdArgs.SetExpiration)
            |> List.map ExpirationSet
        [
            yield! resCancels
            yield! extends
            yield! refunds
            yield! unblocks
            yield LessonDeleted args
        ]
    )

let deleteWorkshop (workshop:Projections.ExistingWorkshop) (args:CmdArgs.DeleteWorkshop) =
    [ WorkshopDeleted args ] |> Ok

let private tryGetUserReservation (lesson:Projections.ExistingLesson) (user:Projections.ExistingUser) =
    lesson.Reservations
    |> List.tryFind (fun x -> x.UserId = user.Id)
    |> Option.map (fun x -> x.UseCredits |> LessonPayment.fromUseCredits)

let addLessonReservation (lesson:Projections.ExistingLesson,user:Projections.ExistingUser) (args:CmdArgs.AddLessonReservation) =
    let lessonStatus = LessonStatus.getLessonStatus lesson.StartDate lesson.IsCancelled lesson.Capacity lesson.Reservations.Length
    let userReservation = tryGetUserReservation lesson user
    let desiredPayment = LessonPayment.fromUseCredits args.UseCredits
    let reservationStatus = ReservationStatus.getReservationStatus lessonStatus lesson.StartDate lesson.CancellableBeforeStart userReservation user.Credits user.CreditsExpiration user.CashReservationBlockedUntil
    
    match reservationStatus with
    | CanBeReserved payment when payment = desiredPayment ->
        [
            yield LessonReservationAdded args
            if args.UseCredits then
                yield CreditWithdrawn { UserId = args.UserId; LessonId = lesson.Id }
            else
                yield CashReservationsBlocked { UserId = args.UserId; Expires = lesson.EndDate; LessonId = lesson.Id }
        ] |> Ok
    | _ -> Error DomainError.LessonCannotBeReserved
            