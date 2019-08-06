module Yobo.FunctionApp.Calendar.Functions

open System
open Yobo.Shared.Calendar.Domain
open FSharp.Rop
open Yobo.Core
open Yobo.Shared.Calendar
open Yobo.Shared.Communication
open Yobo.Shared.Domain

module ArgsBuilder =
    open Yobo.FunctionApp

    let buildAddReservation userId (x:AddReservation) =
        let count,useCredits = x.UserReservation.ToIntAndBool
        ({
            Id = x.LessonId
            UserId = userId
            Count = count
            UseCredits = useCredits
        } : Lessons.CmdArgs.AddReservation)

        
let addReservation getLessonProjection getUserProjection cmdHandler (user:Yobo.Shared.Domain.User,x:AddReservation) =
    result {
        let! lessonProj = getLessonProjection x.LessonId |> Result.ofOption (DomainError.ItemDoesNotExist "Id" |> ServerError.DomainError)
        let! userProj = getUserProjection user.Id |> Result.ofOption (DomainError.ItemDoesNotExist "Id" |> ServerError.DomainError)
        let args = x |> ArgsBuilder.buildAddReservation user.Id
        let! _ = args |> cmdHandler (lessonProj,userProj)
        return ()
    }

let cancelReservation getProjection cmdHandler (user:Yobo.Shared.Domain.User,id:Guid) =
    result {
        let! proj = getProjection id |> Result.ofOption (DomainError.ItemDoesNotExist "Id" |> ServerError.DomainError)
        let! _ = ({ Id = id; UserId = user.Id } : Lessons.CmdArgs.CancelReservation) |> cmdHandler proj
        return ()
    }