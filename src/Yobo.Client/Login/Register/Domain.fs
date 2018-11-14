module Yobo.Client.Login.Register.Domain

open Yobo.Shared.Login.Register.Domain
open Yobo.Shared.Validation

type State = {
    IsRegistering : bool
    AlreadyTried : bool
    Account : Account
    ValidationResult: ValidationResult
}
with
    static member Init = {
        IsRegistering = false
        AlreadyTried = false
        Account = Account.Init
        ValidationResult = ValidationResult.Empty
    }

type Msg =
    | Register
    | ChangeFirstName of string
    | ChangeLastName of string
    | ChangeEmail of string
    | ChangePassword of string
    | ChangeSecondPassword of string