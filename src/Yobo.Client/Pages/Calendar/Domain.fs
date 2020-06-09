module Yobo.Client.Pages.Calendar.Domain

open Yobo.Shared.Core.Reservations.Communication
open Yobo.Shared.Core.Reservations.Domain
open Yobo.Shared.Errors

type Model = {
    Lessons : Queries.Lesson list
//    Workshops : Yobo.Shared.Domain.Workshop list
    WeekOffset : int
}

module Model =
    let init = {
        WeekOffset = 0
        Lessons = []
    }

type Msg =
    | Init
    | LoadLessons
    | LessonsLoaded of ServerResult<Queries.Lesson list>
    | WeekOffsetChanged of int
    | AddReservation of Request.AddReservation
    | ReservationAdded of ServerResult<unit>
//    | LoadWorkshops
//    | WorkshopsLoaded of ServerResult<Yobo.Shared.Domain.Workshop list>
//    | CancelReservation of Guid
//    | ReservationCancelled of ServerResult<unit>