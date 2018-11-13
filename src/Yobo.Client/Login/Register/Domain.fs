module Yobo.Client.Login.Register.Domain

type Account = {
    FirstName: string
    LastName: string
    Email: string
    Password: string
    SecondPassword: string
}
with
    static member Init = {
        FirstName = ""
        LastName = ""
        Email = ""
        Password = ""
        SecondPassword = ""
    }

type State = {
    IsRegistering : bool
    Account : Account
}
with
    static member Init = {
        IsRegistering = false
        Account = Account.Init
    }

type Msg =
    | Register
    | ChangeFirstName of string
    | ChangeLastName of string
    | ChangeEmail of string
    | ChangePassword of string
    | ChangeSecondPassword of string