module Yobo.Shared.Login.Register.Validation

open System
open Yobo.Shared.Login.Register.Domain

let validateNotEmpty msg name value errors = if String.IsNullOrEmpty(value) then (name, msg) :: errors else errors

let validateAccount (acc:Account) =
    []
    |> validateNotEmpty "Blablab" "FirstName" acc.FirstName
    |> validateNotEmpty "Blablab" "LastName" acc.LastName