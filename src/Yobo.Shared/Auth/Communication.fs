module Yobo.Shared.Auth.Communication

open Yobo.Shared.Communication

[<RequireQualifiedAccess>]
module Request =
    type Login = {
        Email : string
        Password : string
    }
    
    module Login =
        let init = { Email = ""; Password = "" }

type AuthService = {
    GetToken : Request.Login -> ServerResponse<string>
    RefreshToken : string -> ServerResponse<string>
}
with
    static member RouteBuilder _ m = sprintf "/api/auth/%s" m