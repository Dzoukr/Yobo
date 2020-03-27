module Yobo.Shared.Users.Communication

open Domain

type UsersService = {
    GetAllUsers : unit -> Async<Queries.User list>
}
with
    static member RouteBuilder _ m = sprintf "/api/users/%s" m