module Yobo.Shared.UserAccount.Communication

open System
open Yobo.Shared.Domain

[<RequireQualifiedAccess>]
module Response =
    type UserInfo = {
        Id : Guid
        FirstName : string
        LastName : string
    }

type UserAccountService = {
    GetUserInfo : SecuredParam<unit> -> ServerResponse<Response.UserInfo>
}
with
    static member RouteBuilder _ m = sprintf "/api/useraccount/%s" m