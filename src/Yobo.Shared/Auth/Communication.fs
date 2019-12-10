module Yobo.Shared.Auth.Communication

open Yobo.Shared.Communication

[<RequireQualifiedAccess>]
module Request =
    type Login = {
        Email : string
        Password : string
    }

type API = {
    Login : Request.Login -> ServerResponse<string>
}