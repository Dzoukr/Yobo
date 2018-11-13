module Yobo.Client.Login.Domain

open Yobo.Shared

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
    Page : Router.Page
    IsLogging: bool
    Credentials: Credentials
}
with
    static member Init = {
        Page = Router.Page.Login
        IsLogging = false
        Credentials = Credentials.Init
    }

type Msg =
    | Login
    | EmailChange of string
    | PasswordChange of string