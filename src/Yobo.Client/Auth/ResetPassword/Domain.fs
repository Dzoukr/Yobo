module Yobo.Client.Auth.ResetPassword.Domain

open System
open Yobo.Shared.Communication

type FormState = {
    Password : string
    PasswordAgain : string
}

type State = {
    Form : FormState
    PasswordResetKey : Guid
    ResetResult : ServerResult<unit> option
}
with
    static member Init key = {
        Form = { Password = ""; PasswordAgain = "" }
        PasswordResetKey = key
        ResetResult = None
    }

type Msg =
    | FormChanged of FormState
    | ResetPassword