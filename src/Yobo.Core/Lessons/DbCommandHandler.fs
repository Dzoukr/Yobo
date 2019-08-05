module Yobo.Core.Lessons.DbCommandHandler

open System
open Projections
open Yobo.Shared.Domain
open FSharp.Rop

let private tryFindById (allLessons:ExistingLesson list) (id:Guid) =
    allLessons
    |> List.tryFind (fun x -> x.Id = id)

let private onlyIfNotAlreadyCancelled lesson =
    if lesson.IsCancelled then DomainError.LessonIsAlreadyCancelled |> Error
    else Ok lesson

let private onlyIfNotFull count lesson =
    let foldFn acc { Count = item } = acc + item
    let res = lesson.Reservations |> List.fold foldFn 0
    if res + count > Yobo.Shared.Calendar.Domain.maxCapacity then
        DomainError.LessonIsFull |> Error
    else Ok lesson

let private onlyIfUserNotAlreadyReserved userId lesson =
    lesson.Reservations
    |> List.tryFind (fun ({UserId = u}) -> u = userId)
    |> function
        | Some _ -> DomainError.LessonIsAlreadyReserved |> Error
        | None -> Ok lesson

let private onlyIfNotAlreadyStarted (lesson:ExistingLesson) =
    if lesson.StartDate < DateTimeOffset.Now then
        DomainError.LessonIsClosed |> Error
    else Ok lesson

let private onlyIfCanBeCancelled userId lesson =
    lesson.Reservations
    |> List.tryFind (fun ({UserId=u}) -> u = userId)
    |> function
        | Some _ ->
            let limit = lesson.StartDate |> Yobo.Shared.Calendar.Domain.getCancellingDate
            if DateTimeOffset.Now > limit then
                DomainError.LessonCancellingIsClosed |> Error
            else Ok lesson
        | None -> DomainError.LessonIsNotReserved |> Error

let create (allLessons:ExistingLesson list) (args:CmdArgs.Create) =
    match tryFindById allLessons args.Id with
    | Some e -> DomainError.ItemAlreadyExists "Id" |> Error
    | None -> [ Created args ] |> Ok

let cancel (lesson:ExistingLesson) (args:CmdArgs.Cancel) =
    lesson
    |> onlyIfNotAlreadyCancelled
    <!> (fun lsn ->
        let refund,unblock = lsn.Reservations |> List.partition (fun x -> x.UseCredits)
        let refunds = 
            refund 
            |> List.map ((fun x -> { UserId = x.UserId; Amount = x.Count; LessonId = lsn.Id } : CmdArgs.RefundCredits ) >> CreditsRefunded)
        let unblocks = 
            unblock 
            |> List.map ((fun x -> { UserId = x.UserId } : CmdArgs.UnblockCashReservations) >> CashReservationsUnblocked)
        [ 
            yield! refunds
            yield! unblocks
            yield Cancelled args 
        ]
    )

let addReservation (lesson:ExistingLesson) (args:CmdArgs.AddReservation) =
    lesson
    |> onlyIfNotFull args.Count
    >>= onlyIfUserNotAlreadyReserved args.UserId
    >>= onlyIfNotAlreadyStarted
    <!> (fun _ -> 
        [
            yield ReservationAdded args
            if args.UseCredits then
                yield CreditsWithdrawn { UserId = args.UserId; Amount = args.Count; LessonId = lesson.Id }
            else
                yield CashReservationsBlocked { UserId = args.UserId; Expires = lesson.EndDate; LessonId = lesson.Id }
        ]
    )

let cancelReservation (lesson:ExistingLesson) (args:CmdArgs.CancelReservation) =
    lesson
    |> onlyIfCanBeCancelled args.UserId
    <!> (fun l -> 
        let userReservation = l.Reservations |> List.find (fun x -> x.UserId = args.UserId)
        [
            yield ReservationCancelled args
            if userReservation.UseCredits then
                yield CreditsRefunded { UserId = args.UserId; Amount = userReservation.Count; LessonId = l.Id }
            else 
                yield CashReservationsUnblocked { UserId = args.UserId }
        ]
    )