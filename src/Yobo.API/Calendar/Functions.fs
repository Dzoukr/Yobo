module Yobo.API.Calendar.Functions

open System
open Yobo.Shared.Calendar.Domain
open FSharp.Rop
open Yobo.Core.Users
open Yobo.Core
open Yobo.Shared.Calendar
open Yobo.Shared.Communication

module ArgsBuilder =
    open Yobo.API

    let buildAddReservation userId (x:AddReservation) =
        let count,useCredits = x.UserReservation.ToIntAndBool
        ({
            Id = x.LessonId
            UserId = userId
            Count = count
            UseCredits = useCredits
        } : Lessons.CmdArgs.AddReservation)

        
let addReservation userId cmdHandler (x:AddReservation) =
    result {
        let args = x |> ArgsBuilder.buildAddReservation userId
        let! _ = args |> (Lessons.Command.AddReservation >> CoreCommand.Lessons >> cmdHandler)
        return ()
    }

let cancelReservation userId cmdHandler (x:Guid) =
    result {
        let! _ = ({ Id = x; UserId = userId } : Lessons.CmdArgs.CancelReservation) |> Lessons.CancelReservation |> CoreCommand.Lessons |> cmdHandler
        return ()
    }