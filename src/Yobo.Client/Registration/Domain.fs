module Yobo.Client.Registration.Domain

open System
open Yobo.Shared.Registration.Domain
open Yobo.Shared.Validation
open Yobo.Shared.Communication

type State = {
    IsRegistrationing : bool
    AlreadyTried : bool
    Account : Account
    ValidationResult: ValidationResult
    RegistrationResult: Result<Guid, ServerError> option
}
with
    static member Init = {
        IsRegistrationing = false
        AlreadyTried = false
        Account = Account.Init
        ValidationResult = ValidationResult.Empty
        RegistrationResult = None
    }

type Msg =
    | Register
    | RegisterDone of Result<Guid,ServerError>
    | ChangeFirstName of string
    | ChangeLastName of string
    | ChangeEmail of string
    | ChangePassword of string
    | ChangeSecondPassword of string