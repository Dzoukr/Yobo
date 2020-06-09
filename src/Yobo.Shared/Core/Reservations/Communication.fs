module Yobo.Shared.Core.Reservations.Communication

open System
open Domain
open Yobo.Shared.Core.Domain

[<RequireQualifiedAccess>]
module Request =
    type AddReservation = {
        LessonId : Guid
        Payment : LessonPayment
    }

type ReservationsService = {
    GetLessons : int -> Async<Queries.Lesson list>
    AddReservation : Request.AddReservation -> Async<unit>
}
with
    static member RouteBuilder _ m = sprintf "/api/reservations/%s" m