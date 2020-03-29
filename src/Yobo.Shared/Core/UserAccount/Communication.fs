module Yobo.Shared.Core.UserAccount.Communication

open Domain

type UserAccountService = {
    GetUserInfo : unit -> Async<Queries.UserAccount>
}
with
    static member RouteBuilder _ m = sprintf "/api/useraccount/%s" m