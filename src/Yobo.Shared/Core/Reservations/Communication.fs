module Yobo.Shared.Core.Reservations.Communication

open System
open Domain

type ReservationsService = {
    GetLessons : int -> Async<Queries.Lesson list>
    GetOnlineLessons : int -> Async<Queries.OnlineLesson list>
}
with
    static member RouteBuilder _ m = sprintf "/api/reservations/%s" m