module Yobo.Shared.Auth.Domain

open System

type LoggedUser = {
    Id : Guid
    Email : string
    FirstName : string
    LastName : string
    IsAdmin : bool
}

type NewAccount = {
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

type Login = {
    Email: string
    Password: string
}
with
    static member Init = {
        Email = ""
        Password = ""
    }