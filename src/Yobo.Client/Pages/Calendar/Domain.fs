module Yobo.Client.Pages.Calendar.Domain

open Yobo.Shared.Core.Reservations.Domain
open Yobo.Shared.Errors

type Model = {
    Lessons : Queries.Lesson list
    OnlineLessons : Queries.OnlineLesson list
//    Workshops : Yobo.Shared.Domain.Workshop list
    WeekOffset : int
}

module Model =
    let init = {
        WeekOffset = 0
        Lessons = []
        OnlineLessons = []
    }

type Msg =
    | Init
    | LoadLessons
    | LessonsLoaded of ServerResult<Queries.Lesson list>
    | LoadOnlineLessons
    | OnlineLessonsLoaded of ServerResult<Queries.OnlineLesson list>
    | WeekOffsetChanged of int
//    | LoadWorkshops
//    | WorkshopsLoaded of ServerResult<Yobo.Shared.Domain.Workshop list>
//    | AddReservation of AddReservation
//    | ReservationAdded of ServerResult<unit>
//    | CancelReservation of Guid
//    | ReservationCancelled of ServerResult<unit>