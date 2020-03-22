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
    GetToken : Request.Login -> Async<string>
    RefreshToken : string -> Async<string>
    Register : Request.Register -> Async<unit>
    ActivateAccount : Guid -> Async<unit>
    ForgottenPassword : Request.ForgottenPassword -> Async<unit>
    ResetPassword : Request.ResetPassword -> Async<unit>
}
with
    static member RouteBuilder _ m = sprintf "/api/auth/%s" m