module Yobo.Shared.Auth.Domain

open System

type NewAccount = {
    FirstName: string
    LastName: string
    Email: string
    Password: string
    SecondPassword: string
    AgreeButtonChecked : bool
}
with
    static member Init = {
        FirstName = ""
        LastName = ""
        Email = ""
        Password = ""
        SecondPassword = ""
        AgreeButtonChecked = false
    }

type Login = {
    Email: string
    Password: string
}
with
    static member Init = {
        Email = ""
        Password = ""
    }

type PasswordReset = {
    Password: string
    SecondPassword: string
}
with
    static member Init = {
        Password = ""
        SecondPassword = ""
    }