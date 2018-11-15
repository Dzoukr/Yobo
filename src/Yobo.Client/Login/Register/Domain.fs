module Yobo.Client.Login.Register.Domain

open System
open Yobo.Shared.Login.Register.Domain
open Yobo.Shared.Validation
open Yobo.Shared.Communication

type State = {
    IsRegistering : bool
    AlreadyTried : bool
    Account : Account
    ValidationResult: ValidationResult
    RegistrationResult: Result<Guid, ServerError>
}
with
    static member Init = {
        IsRegistering = false
        AlreadyTried = false
        Account = Account.Init
        ValidationResult = ValidationResult.Empty
        RegistrationResult = Ok Guid.Empty
    }

type Msg =
    | Register
    | RegisterDone of Result<Guid,ServerError>
    | ChangeFirstName of string
    | ChangeLastName of string
    | ChangeEmail of string
    | ChangePassword of string
    | ChangeSecondPassword of string