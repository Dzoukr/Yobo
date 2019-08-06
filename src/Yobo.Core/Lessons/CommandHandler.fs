module Yobo.Core.Lessons.CommandHandler

open System
open Projections
open Yobo.Shared.Domain
open FSharp.Rop

let private tryFindById (allLessons:ExistingLesson list) (id:Guid) =
    allLessons
    |> List.tryFind (fun x -> x.Id = id)

let private tryFindWorkshopById (allWorkshops:ExistingWorkshop list) (id:Guid) =
    allWorkshops
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

let private onlyIfActivated (user:Projections.ExistingUser) =
    if user.IsActivated then Ok user else DomainError.UserNotActivated |> Error

let private onlyIfEnoughCredits amount (user:Projections.ExistingUser) =
    if user.Credits - amount < 0 then DomainError.NotEnoughCredits |> Error else Ok user

let private onlyIfNotAlreadyBlocked state =
    match state.CashReservationsBlockedUntil with
    | None -> Ok state
    | Some d ->
        if d < DateTimeOffset.Now then Ok state else DomainError.CashReservationIsBlocked |> Error

let createLesson (allLessons:ExistingLesson list) (args:CmdArgs.CreateLesson) =
    match tryFindById allLessons args.Id with
    | Some e -> DomainError.ItemAlreadyExists "Id" |> Error
    | None -> [ LessonCreated args ] |> Ok

let cancelLesson (lesson:ExistingLesson) (args:CmdArgs.CancelLesson) =
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
            yield LessonCancelled args 
        ]
    )

let addReservation (lesson:ExistingLesson,user:Projections.ExistingUser) (args:CmdArgs.AddReservation) =
    lesson
    |> onlyIfNotFull args.Count
    >>= onlyIfUserNotAlreadyReserved args.UserId
    >>= onlyIfNotAlreadyStarted
    >>= (fun _ -> if args.UseCredits then onlyIfEnoughCredits args.Count user else onlyIfNotAlreadyBlocked user)
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

let addCredits (user:Projections.ExistingUser) (args:CmdArgs.AddCredits) =
    user
    |> onlyIfActivated
    <!> (fun _ -> CreditsAdded args)
    <!> List.singleton

let createWorkshop (allWorkshops:ExistingWorkshop list) (args:CmdArgs.CreateWorkshop) =
    match tryFindWorkshopById allWorkshops args.Id with
    | Some e -> DomainError.ItemAlreadyExists "Id" |> Error
    | None -> [ WorkshopCreated args ] |> Ok

let deleteWorkshop (workshop:ExistingWorkshop) (args:CmdArgs.DeleteWorkshop) =
    WorkshopDeleted args
    |> List.singleton
    |> Ok