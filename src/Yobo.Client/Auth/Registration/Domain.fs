module Yobo.Client.Auth.Registration.Domain

open System
open Yobo.Shared.Auth.Domain
open Yobo.Shared.Validation
open Yobo.Shared.Communication

type State = {
    IsRegistrating : bool
    AlreadyTried : bool
    Account : NewAccount
    ValidationResult: ValidationResult
    RegistrationResult: ServerResult<Guid> option
    ShowTerms : bool
}
with
    static member Init = {
        IsRegistrating = false
        AlreadyTried = false
        Account = NewAccount.Init
        ValidationResult = ValidationResult.Empty
        RegistrationResult = None
        ShowTerms = false
    }

type Msg =
    | Register
    | Registered of ServerResult<Guid>
    | ChangeFirstName of string
    | ChangeLastName of string
    | ChangeEmail of string
    | ChangePassword of string
    | ChangeSecondPassword of string
    | ToggleAgreement
    | ToggleTerms

