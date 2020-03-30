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
    }

let private tryFindById (allLessons:Projections.ExistingLesson list) (id:Guid) =
    allLessons
    |> List.tryFind (fun x -> x.Id = id)

let private onlyIfActivated (user:Projections.ExistingUser) =
    if user.IsActivated then Ok user else DomainError.UserNotActivated |> Error
    
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
    
let createLesson (allLessons:Projections.ExistingLesson list) (args:CmdArgs.CreateLesson) =
    match tryFindById allLessons args.Id with
    | Some e -> DomainError.LessonAlreadyExists |> Error
    | None -> [ LessonCreated args ] |> Ok                