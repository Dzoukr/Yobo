module Yobo.Shared.Auth.Validation

open Yobo.Shared.Validation
open Yobo.Shared.Auth.Domain

let validateAccount (acc:NewAccount) =
    [
        "FirstName", validateNotEmpty (fun (x:NewAccount) -> x.FirstName)
        "LastName", validateNotEmpty (fun x -> x.LastName)
        "Email", validateNotEmpty (fun x -> x.Email)
        "Password", validateLongerThan 5 (fun x -> x.Password)
        "SecondPassword", validateLongerThan 5 (fun x -> x.SecondPassword)
        "Password", validateEquals "SecondPassword" (fun x -> x.Password) (fun x -> x.SecondPassword)
        "Email", validateEmail (fun x -> x.Email)
        "Terms", validateTermsAgreed  (fun x -> x.AgreeButtonChecked)
    ] |> validate acc

let validateLogin (log:Login) =
    [
        "Password", validateNotEmpty (fun x -> x.Password)
        "Email", validateEmail (fun x -> x.Email)
    ] |> validate log