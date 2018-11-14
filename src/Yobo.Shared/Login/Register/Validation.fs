module Yobo.Shared.Login.Register.Validation

open System
open Yobo.Shared.Login.Register.Domain
open Yobo.Shared.Validation

let validateAccount (acc:Account) =
    [
        validateNotEmpty "Blablab" "FirstName" (fun x -> x.FirstName)
        validateNotEmpty "Blablab" "LastName" (fun x -> x.LastName)
        validateLongerThan 5 "Heslo musí být delší než 5 znaků" "Password" (fun x -> x.Password)
    ] |> validate acc