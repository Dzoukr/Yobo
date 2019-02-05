module Yobo.Client.Auth.ResetPassword.Domain

open System
open Yobo.Shared.Communication
open Yobo.Shared.Validation
open Yobo.Shared.Auth.Domain


type State = {
    Form : PasswordReset
    PasswordResetKey : Guid
    ResetResult : ServerResult<unit> option
    AlreadyTried : bool
    ValidationResult : ValidationResult
}
with
    static member Init key = {
        Form = PasswordReset.Init
        PasswordResetKey = key
        ResetResult = None
        AlreadyTried = false
        ValidationResult = ValidationResult.Empty
    }

type Msg =
    | FormChanged of PasswordReset
    | ResetPassword
    | PasswordReset of ServerResult<unit>