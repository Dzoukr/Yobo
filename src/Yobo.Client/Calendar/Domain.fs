module Yobo.Client.Calendar.Domain

open Yobo.Shared.Calendar.Domain
open Yobo.Shared.Communication
open System

type State = {
    Lessons : Lesson list
    Workshops : Yobo.Shared.Domain.Workshop list
    WeekOffset : int
}
with
    static member Init = {
        Lessons = []
        Workshops = []
        WeekOffset = 0
    }

type Msg =
    | Init
    | LoadUserLessons
    | UserLessonsLoaded of ServerResult<Lesson list>
    | LoadWorkshops
    | WorkshopsLoaded of ServerResult<Yobo.Shared.Domain.Workshop list>
    | WeekOffsetChanged of int
    | AddReservation of AddReservation
    | ReservationAdded of ServerResult<unit>
    | CancelReservation of Guid
    | ReservationCancelled of ServerResult<unit>