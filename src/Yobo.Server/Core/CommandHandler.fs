module Yobo.Server.Core.CommandHandler

open System
open Domain
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
    }
    
    type ExistingWorkshop = {
        Id : Guid
    }
    
    type ExistingOnlineLesson = {
        Id : Guid
        Reservations : UserReservation list
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        IsCancelled : bool
        Capacity : int
    }

let private onlyIfActivated (user:Projections.ExistingUser) =
    if user.IsActivated then Ok user else DomainError.UserNotActivated |> Error

let private onlyIfCanBeCancelled (lsn:Projections.ExistingLesson) =
    if Yobo.Shared.Core.Domain.canLessonBeCancelled lsn.IsCancelled lsn.StartDate then Ok lsn
    else Error DomainError.LessonCannotBeCancelled

let private onlyIfOnlineCanBeCancelled (lsn:Projections.ExistingOnlineLesson) =
    if Yobo.Shared.Core.Domain.canOnlineLessonBeCancelled lsn.IsCancelled lsn.StartDate then Ok lsn
    else Error DomainError.LessonCannotBeCancelled

let private onlyIfCanBeDeleted (lsn:Projections.ExistingLesson) =
    if Yobo.Shared.Core.Domain.canLessonBeDeleted lsn.StartDate then Ok lsn
    else Error DomainError.LessonCannotBeCancelled

let private onlyIfOnlineCanBeDeleted (lsn:Projections.ExistingOnlineLesson) =
    if Yobo.Shared.Core.Domain.canLessonBeDeleted lsn.StartDate then Ok lsn
    else Error DomainError.OnlineLessonCannotBeCancelled

let private onlyIfLessonAvailable (lsn:Projections.ExistingLesson) =
    if lsn.Reservations.Length >= lsn.Capacity then Error DomainError.LessonAlreadyFull
    else if lsn.IsCancelled then Error DomainError.LessonCannotBeReserved
    else if lsn.StartDate < DateTimeOffset.UtcNow then Error DomainError.LessonCannotBeReserved
    else Ok lsn

let private onlyIfUserNotAlreadyReserved userId (lsn:Projections.ExistingLesson) =
    if lsn.Reservations |> List.exists (fun x -> x.UserId = userId) then
        Error DomainError.LessonAlreadyReserved
    else Ok lsn
    
let private onlyIfEnoughCredits (user:Projections.ExistingUser) =
    if user.Credits - 1 < 0 then DomainError.NotEnoughCredits |> Error else Ok user

let private onlyIfNotAfterCreditsExpiration date (user:Projections.ExistingUser) =
    match user.CreditsExpiration with
    | None -> Ok user
    | Some exp ->
        if date > exp then Error DomainError.CreditsExpiresBeforeLessonStart
        else Ok user

let private onlyIfNotAlreadyBlocked (user:Projections.ExistingUser) =
    match user.CashReservationBlockedUntil with
    | None -> Ok user
    | Some d ->
        if d < DateTimeOffset.Now then Ok user else DomainError.CashReservationIsBlocked |> Error

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
    
let createOnlineLesson (args:CmdArgs.CreateOnlineLesson) =
    [ OnlineLessonCreated args ] |> Ok

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
    
let changeOnlineLessonDescription (lesson:Projections.ExistingOnlineLesson) (args:CmdArgs.ChangeOnlineLessonDescription) =
    [ OnlineLessonDescriptionChanged args ] |> Ok
    
let cancelOnlineLesson (lesson:Projections.ExistingOnlineLesson) (args:CmdArgs.CancelOnlineLesson) =
    lesson
    |> onlyIfOnlineCanBeCancelled
    |>> (fun lsn ->
        let resCancels = lsn.Reservations |> List.map ((fun x -> { OnlineLessonId = lsn.Id; UserId = x.UserId } : CmdArgs.CancelOnlineLessonReservation) >> OnlineLessonReservationCancelled) 
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
            yield OnlineLessonCancelled args
        ]
    )
    
let deleteOnlineLesson (lesson:Projections.ExistingOnlineLesson) (args:CmdArgs.DeleteOnlineLesson) =
    lesson
    |> onlyIfOnlineCanBeDeleted
    |>> (fun lsn ->
        let resCancels = lsn.Reservations |> List.map ((fun x -> { OnlineLessonId = lsn.Id; UserId = x.UserId } : CmdArgs.CancelOnlineLessonReservation) >> OnlineLessonReservationCancelled) 
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
            yield OnlineLessonDeleted args
        ]
    )

let addLessonReservation (lesson:Projections.ExistingLesson,user:Projections.ExistingUser) (args:CmdArgs.AddLessonReservation) =
    lesson
    |> onlyIfLessonAvailable
    >>= onlyIfUserNotAlreadyReserved args.UserId
    >>= (fun _ -> if args.UseCredits then onlyIfEnoughCredits user else onlyIfNotAlreadyBlocked user)
    >>= (fun _ -> if args.UseCredits then onlyIfNotAfterCreditsExpiration lesson.StartDate user else Ok user)
    |>> (fun _ ->
        [
            yield LessonReservationAdded args
            if args.UseCredits then
                yield CreditWithdrawn { UserId = args.UserId; LessonId = lesson.Id }
            else
                yield CashReservationsBlocked { UserId = args.UserId; Expires = lesson.EndDate; LessonId = lesson.Id }
        ]
    )               