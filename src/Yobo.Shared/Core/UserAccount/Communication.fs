module Yobo.Shared.Core.UserAccount.Communication

open Domain

type UserAccountService = {
    GetUserInfo : unit -> Async<Queries.UserAccount>
    GetMyLessons : unit -> Async<Queries.Lesson list>
    GetMyOnlineLessons : unit -> Async<Queries.OnlineLesson list>
}
with
    static member RouteBuilder _ m = sprintf "/api/useraccount/%s" m