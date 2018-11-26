module Yobo.Client.Login.Domain

type Credentials = {
    Email: string
    Password: string
}
with
    static member Init = {
        Email = ""
        Password = ""
    }

type State = {
    IsLogging : bool
    Credentials : Credentials
}
with
    static member Init = {
        IsLogging = false
        Credentials = Credentials.Init
    }

type Msg =
    | Login
    | ChangeEmail of string
    | ChangePassword of string