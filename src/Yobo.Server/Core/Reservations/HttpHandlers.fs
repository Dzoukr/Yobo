module Yobo.Server.Core.Reservations.HttpHandlers

open System
open Giraffe
open Yobo.Server
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open FSharp.Control.Tasks
open Yobo.Shared.Errors
open Yobo.Shared.Core.Admin.Communication
open Yobo.Shared.Core.Admin.Validation
open Yobo.Shared.Core.Admin.Domain
open Yobo.Server.Core.Domain
open Yobo.Libraries.DateTime
open Yobo.Shared.Core.Reservations.Communication
open Yobo.Shared.DateTime

let private offsetToDateRange (offset:int) =
    let safeOffset =
        if offset < Yobo.Shared.Core.Reservations.Domain.minWeekOffset then Yobo.Shared.Core.Reservations.Domain.minWeekOffset
        else offset
    safeOffset |> DateRange.getDateRangeForWeekOffset        

let private reservationsService (root:CompositionRoot) userId : ReservationsService =
    {
        GetLessons = offsetToDateRange >> root.Reservations.Queries.GetLessons userId >> Async.AwaitTask
        GetOnlineLessons = offsetToDateRange >> root.Reservations.Queries.GetOnlineLessons userId >> Async.AwaitTask
    }

let reservationsServiceHandler (root:CompositionRoot) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder ReservationsService.RouteBuilder
    |> Remoting.fromContext (Auth.HttpHandlers.withUser (reservationsService root))
    |> Remoting.withErrorHandler Remoting.errorHandler
    |> Remoting.buildHttpHandler