module Yobo.Shared.Login.Register.Validation

open Yobo.Shared.Login.Register.Domain
open Yobo.Shared.Validation
open Yobo.Shared.Text

let validateAccount (acc:Account) =
    [
        validateNotEmpty FirstName (fun x -> x.FirstName)
        validateNotEmpty LastName (fun x -> x.LastName)
        validateNotEmpty Email (fun x -> x.Email)
        validateLongerThan 5 Password (fun x -> x.Password)
        validateLongerThan 5 SecondPassword (fun x -> x.SecondPassword)
        validateEquals Password SecondPassword (fun x -> x.Password) (fun x -> x.SecondPassword)
        validateEmail Email (fun x -> x.Email)
    ] |> validate acc