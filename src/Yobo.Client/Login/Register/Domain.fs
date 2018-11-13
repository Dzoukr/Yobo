module Yobo.Client.Login.Register.Domain

open Yobo.Shared.Login.Register.Domain

type State = {
    IsRegistering : bool
    Account : Account
    ValidationErrors: (string * string) list
}
with
    static member Init = {
        IsRegistering = false
        Account = Account.Init
        ValidationErrors = []
    }

type Msg =
    | Register
    | ChangeFirstName of string
    | ChangeLastName of string
    | ChangeEmail of string
    | ChangePassword of string
    | ChangeSecondPassword of string