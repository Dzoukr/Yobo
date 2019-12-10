module Yobo.Shared.Auth.Communication

open Yobo.Shared.Communication

[<RequireQualifiedAccess>]
module Request =
    type Login = {
        Email : string
        Password : string
    }

type AuthService = {
    Login : Request.Login -> ServerResponse<string>
}
with
    static member RouteBuilder _ m = sprintf "/api/auth/%s" m