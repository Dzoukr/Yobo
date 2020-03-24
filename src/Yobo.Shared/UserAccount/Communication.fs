module Yobo.Shared.UserAccount.Communication

open System
open Yobo.Shared.Domain
open Domain

type UserAccountService = {
    GetUserInfo : unit -> Async<Queries.UserAccount>
}
with
    static member RouteBuilder _ m = sprintf "/api/useraccount/%s" m