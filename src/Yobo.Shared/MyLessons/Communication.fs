module Yobo.Shared.MyLessons.Communication

open System
open Domain
open Yobo.Shared.Communication

let routeBuilder _ m = sprintf "/api/mylessons/%s" m

type API = {
    GetMyLessons : SecuredParam<unit> -> ServerResponse<Lesson list>
} 