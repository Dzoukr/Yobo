module Yobo.Shared.Auth.Validation

open Yobo.Shared.Validation
open Yobo.Shared.Auth.Domain

let validateAccount (acc:NewAccount) =
    [
        validateNotEmpty "FirstName" (fun (x:NewAccount) -> x.FirstName)
        validateNotEmpty "LastName" (fun x -> x.LastName)
        validateNotEmpty "Email" (fun x -> x.Email)
        validateLongerThan 5 "Password" (fun x -> x.Password)
        validateLongerThan 5 "SecondPassword" (fun x -> x.SecondPassword)
        validateEquals "Password" "SecondPassword" (fun x -> x.Password) (fun x -> x.SecondPassword)
        validateEmail "Email" (fun x -> x.Email)
        validateTermsAgreed "Terms" (fun x -> x.AgreeButtonChecked)
    ] |> validate acc

let validateLogin (log:Login) =
    [
        validateNotEmpty "Password" (fun x -> x.Password)
        validateEmail "Email" (fun x -> x.Email)
    ] |> validate log