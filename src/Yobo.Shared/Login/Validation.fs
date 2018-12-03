module Yobo.Shared.Login.Validation

open Yobo.Shared.Validation
open Yobo.Shared.Text
open Domain

let validateAccount (log:Login) =
    [
        validateNotEmpty Password (fun x -> x.Password)
        validateEmail Email (fun x -> x.Email)
    ] |> validate log