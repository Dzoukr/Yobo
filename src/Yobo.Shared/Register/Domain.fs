module Yobo.Shared.Register.Domain

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