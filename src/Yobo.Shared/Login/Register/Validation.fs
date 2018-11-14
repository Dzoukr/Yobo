module Yobo.Shared.Login.Register.Validation

open System
open Yobo.Shared.Login.Register.Domain
open Yobo.Shared.Validation
open Yobo.Shared.Text

let validateAccount (acc:Account) =
    [
        FirstName, validateNotEmpty (fun x -> x.FirstName)
        LastName, validateNotEmpty (fun x -> x.LastName)
        Email, validateNotEmpty (fun x -> x.Email)
        Password, validateLongerThan 5 (fun x -> x.Password)
        SecondPassword, validateLongerThan 5 (fun x -> x.SecondPassword)
        SecondPassword, validateEquals (fun x -> x.Password) (fun x -> x.SecondPassword)
    ] |> validate acc