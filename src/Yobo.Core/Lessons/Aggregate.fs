module Yobo.Core.Lessons.Aggregate

open FSharp.Rop
open Yobo.Shared.Domain
open Yobo.Core.Lessons
open System

let private onlyIfDoesNotExist state =
    if state.Id = State.Init.Id then Ok state
    else DomainError.ItemAlreadyExists "Id" |> Error

let private onlyIfDoesExist state =
    if state.Id <> State.Init.Id then Ok state
    else DomainError.ItemAlreadyExists "Id" |> Error

let private onlyIfNotFull count state =
    let foldFn acc (_,item,_) = acc + item
    let res = state.Reservations |> List.fold foldFn 0
    if res + count > Yobo.Shared.Calendar.Domain.maxCapacity then
        DomainError.LessonIsFull |> Error
    else Ok state

let private onlyIfUserNotAlreadyReserved userId state =
    state.Reservations
    |> List.tryFind (fun (u,_,_) -> u = userId)
    |> function
        | Some _ -> DomainError.LessonIsAlreadyReserved |> Error
        | None -> Ok state

let private onlyIfNotAlreadyStarted state =
    if state.StartDate < DateTimeOffset.Now then
        DomainError.LessonIsClosed |> Error
    else Ok state

let private onlyIfCanBeCancelled userId state =
    state.Reservations
    |> List.tryFind (fun (u,_,_) -> u = userId)
    |> function
        | Some r ->
            let limit = state.StartDate |> Yobo.Shared.Calendar.Domain.getCancellingDate
            if DateTimeOffset.Now > limit then
                DomainError.LessonCancellingIsClosed |> Error
            else Ok state
        | None -> DomainError.LessonIsNotReserved |> Error


let execute (state:State) = function
    | Create args ->
        onlyIfDoesNotExist state
        <!> (fun _ -> Created args)
        <!> List.singleton
    | AddReservation args ->
        onlyIfDoesExist state
        >>= onlyIfNotFull args.Count
        >>= onlyIfUserNotAlreadyReserved args.UserId
        >>= onlyIfNotAlreadyStarted
        <!> (fun _ -> ReservationAdded args)
        <!> List.singleton
    | CancelReservation args ->
        onlyIfDoesExist state
        >>= onlyIfCanBeCancelled args.UserId
        <!> (fun _ -> ReservationCancelled args)
        <!> List.singleton
    
let apply (state:State) = function
    | Created args -> { state with Id = args.Id; StartDate = args.StartDate; EndDate = args.EndDate }
    | ReservationAdded args -> { state with Reservations = (args.UserId, args.Count, args.UseCredits) :: state.Reservations}
    | ReservationCancelled args -> { state with Reservations = state.Reservations |> List.filter (fun (x,_,_) -> x <> args.UserId ) }
