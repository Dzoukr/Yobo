module Yobo.Core.Lessons.Aggregate

open FSharp.Rop
open Yobo.Shared.Domain
open Yobo.Core.Lessons

let private onlyIfDoesNotExist state =
    if state.Id = State.Init.Id then Ok state
    else DomainError.ItemAlreadyExists "Id" |> Error

let private onlyIfDoesExist state =
    if state.Id <> State.Init.Id then Ok state
    else DomainError.ItemAlreadyExists "Id" |> Error

let private onlyIfNotFull count state =
    let foldFn acc item = acc + (snd item)
    let res = state.Reservations |> List.fold foldFn 0
    if res + count > Yobo.Shared.Calendar.Domain.maxCapacity then
        DomainError.LessonIsFull |> Error
    else Ok state

let private onlyIfUserNotAlreadyReserved userId state =
    state.Reservations
    |> List.tryFind (fun (u,_) -> u = userId)
    |> function
        | Some _ -> DomainError.LessonIsAlreadyReserved |> Error
        | None -> Ok state

let execute (state:State) = function
    | Create args ->
        onlyIfDoesNotExist state
        <!> (fun _ -> Created args)
        <!> List.singleton
    | AddReservation args ->
        onlyIfDoesExist state
        >>= onlyIfNotFull args.Count
        >>= onlyIfUserNotAlreadyReserved args.UserId
        <!> (fun _ -> ReservationAdded args)
        <!> List.singleton
        
    
let apply (state:State) = function
    | Created args -> { state with Id = args.Id }
    | ReservationAdded args -> { state with Reservations = (args.UserId, args.Count) :: state.Reservations}