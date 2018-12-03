module Yobo.Shared.Login.Domain

open System

type Login = {
    Email: string
    Password: string
}
with
    static member Init = {
        Email = ""
        Password = ""
    }

type User = {
    Id : Guid
    Email : string
    FirstName : string
    LastName : string
}