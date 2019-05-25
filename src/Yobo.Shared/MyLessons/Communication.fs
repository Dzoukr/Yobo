module Yobo.Shared.MyLessons.Communication

open System
open Domain
open Yobo.Shared.Communication

let routeBuilder _ m = sprintf "/api/mylessons/%s" m

type API = {
    GetLessonsForDateRange : SecuredParam<DateTimeOffset * DateTimeOffset> -> ServerResponse<Lesson list>
} 