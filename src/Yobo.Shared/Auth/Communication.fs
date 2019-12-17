module Yobo.Shared.Auth.Communication

open System
open Yobo.Shared.Domain

[<RequireQualifiedAccess>]
module Request =
    type Login = {
        Email : string
        Password : string
    }
    
    module Login =
        let init = { Email = ""; Password = "" }
        
    type Register = {
        FirstName: string
        LastName: string
        Email: string
        Password: string
        SecondPassword: string
        AgreeButtonChecked : bool
        NewslettersButtonChecked : bool
    }
    
    module Register =
        let init = {
            FirstName = ""
            LastName = ""
            Email = ""
            Password = ""
            SecondPassword = ""
            AgreeButtonChecked = false
            NewslettersButtonChecked = false
        }

type AuthService = {
    GetToken : Request.Login -> ServerResponse<string>
    RefreshToken : string -> ServerResponse<string>
    Register : Request.Register -> ServerResponse<Guid>
}
with
    static member RouteBuilder _ m = sprintf "/api/auth/%s" m