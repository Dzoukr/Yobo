module Yobo.Shared.UserAccount.Communication

open System
open Yobo.Shared.Domain

type UserAccountService = {
    GetUserInfo : SecuredParam<unit> -> ServerResponse<string>
}
with
    static member RouteBuilder _ m = sprintf "/api/useraccount/%s" m