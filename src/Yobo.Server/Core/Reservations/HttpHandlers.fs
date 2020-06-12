module Yobo.Server.Core.Reservations.HttpHandlers

open System
open Giraffe
open Yobo.Server
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open FSharp.Control.Tasks
open Yobo.Shared.Core.Domain
open Yobo.Shared.Core.Admin.Communication
open Yobo.Server.Core.Domain
open Yobo.Shared.Core.Reservations.Communication
open Yobo.Shared.DateTime

let private offsetToDateRange (offset:int) =
    let safeOffset =
        if offset < Yobo.Shared.Core.Reservations.Domain.minWeekOffset then Yobo.Shared.Core.Reservations.Domain.minWeekOffset
        else offset
    safeOffset |> DateRange.getDateRangeForWeekOffset        

let private addReservation (root:ReservationsRoot) userId (r:Request.AddReservation) =
    task {
        let args : CmdArgs.AddLessonReservation =
            {
                UserId = userId
                LessonId = r.LessonId
                UseCredits = r.Payment |> LessonPayment.toUseCredits
            }
        return! root.CommandHandler.AddReservation args            
    }

let private cancelReservation (root:ReservationsRoot) userId (lessonId:Guid) =
    task {
        let args : CmdArgs.CancelLessonReservation =
            {
                UserId = userId
                LessonId = lessonId
            }
        return! root.CommandHandler.CancelReservation args
    }

let private reservationsService (root:CompositionRoot) userId : ReservationsService =
    {
        GetLessons = offsetToDateRange >> root.Reservations.Queries.GetLessons userId >> Async.AwaitTask
        GetWorkshops = offsetToDateRange >> root.Reservations.Queries.GetWorkshops >> Async.AwaitTask
        AddReservation = addReservation root.Reservations userId >> Async.AwaitTask
        CancelReservation = cancelReservation root.Reservations userId >> Async.AwaitTask
    }

let reservationsServiceHandler (root:CompositionRoot) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder ReservationsService.RouteBuilder
    |> Remoting.fromContext (Auth.HttpHandlers.withUser (reservationsService root))
    |> Remoting.withErrorHandler Remoting.errorHandler
    |> Remoting.buildHttpHandler