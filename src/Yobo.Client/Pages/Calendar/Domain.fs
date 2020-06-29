module Yobo.Client.Pages.Calendar.Domain

open System
open Yobo.Client.SharedView
open Yobo.Shared.Core.Reservations.Communication
open Yobo.Shared.Core.Reservations.Domain
open Yobo.Shared.Errors

type Model = {
    Lessons : Queries.Lesson list
    Workshops : Queries.Workshop list
    WeekOffset : int
}

type Msg =
    | LoadLessons
    | LessonsLoaded of ServerResult<Queries.Lesson list>
    | WeekOffsetChanged of int
    | AddReservation of Request.AddReservation
    | ReservationAdded of ServerResult<unit>
    | CancelReservation of Guid
    | ReservationCancelled of ServerResult<unit>
    | LoadWorkshops
    | WorkshopsLoaded of ServerResult<Queries.Workshop list>
