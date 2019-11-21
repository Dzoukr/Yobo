module Yobo.Shared.Admin.Communication

open System
open Yobo.Shared.Domain
open Domain
open Yobo.Shared.Communication

let routeBuilder _ m = sprintf "/api/admin/%s" m

type API = {
    GetAllUsers : SecuredParam<unit> -> ServerResponse<User list>
    AddCredits : SecuredParam<AddCredits> -> ServerResponse<unit>
    GetLessonsForDateRange : SecuredParam<DateTimeOffset * DateTimeOffset> -> ServerResponse<Lesson list>
    GetWorkshopsForDateRange : SecuredParam<DateTimeOffset * DateTimeOffset> -> ServerResponse<Workshop list>
    AddLessons : SecuredParam<AddLesson list> -> ServerResponse<unit>
    AddWorkshops : SecuredParam<AddWorkshop list> -> ServerResponse<unit>
    CancelLesson : SecuredParam<Guid> -> ServerResponse<unit>
    DeleteLesson : SecuredParam<Guid> -> ServerResponse<unit>
    DeleteWorkshop : SecuredParam<Guid> -> ServerResponse<unit>
    UpdateLesson : SecuredParam<UpdateLesson> -> ServerResponse<unit>
}