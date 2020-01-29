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
        
    type ForgottenPassword = {
        Email : string
    }
    
    module ForgottenPassword =
        let init = { Email = "" }
    
    type ResetPassword = {
        PasswordResetKey : Guid
        Password: string
        SecondPassword: string
    }
    
    module ResetPassword =
        let init = { Password = ""; SecondPassword = ""; PasswordResetKey = Guid.Empty }
    
type AuthService = {
    GetToken : Request.Login -> ServerResponse<string>
    RefreshToken : string -> ServerResponse<string>
    Register : Request.Register -> ServerResponse<unit>
    ActivateAccount : Guid -> ServerResponse<unit>
    ForgottenPassword : Request.ForgottenPassword -> ServerResponse<unit>
    ResetPassword : Request.ResetPassword -> ServerResponse<unit>
}
with
    static member RouteBuilder _ m = sprintf "/api/auth/%s" m