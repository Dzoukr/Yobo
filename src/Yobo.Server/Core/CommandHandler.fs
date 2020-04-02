module Yobo.Server.Core.CommandHandler

open System
open Domain
open Yobo.Shared.Errors
open FSharp.Rop.Result.Operators

module Projections =
    type ExistingUser = {
        Id : Guid
        IsActivated : bool
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

let private onlyIfActivated (user:Projections.ExistingUser) =
    if user.IsActivated then Ok user else DomainError.UserNotActivated |> Error

//let private onlyIfNotInPast (args:CmdArgs.UpdateLesson) =
//    let now = DateTimeOffset.UtcNow
//    if args.StartDate <= now || args.EndDate <= now then
//        DomainError.CannotMoveLessonToPast |> Error
//    else args |> Ok

    
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

let changeLessonDescription (lessons:Projections.ExistingLesson) (args:CmdArgs.ChangeLessonDescription) =
    [ LessonDescriptionChanged args ] |> Ok
    