module Yobo.Shared.Calendar.Communication

open System
open Domain
open Yobo.Shared.Communication

let routeBuilder _ m = sprintf "/api/calendar/%s" m

type API = {
    GetLessonsForDateRange : SecuredParam<DateTimeOffset * DateTimeOffset> -> ServerResponse<Lesson list>
    AddReservation : SecuredParam<AddReservation> -> ServerResponse<unit>
} 