module Yobo.Client.Validation

open Yobo.Shared.Validation

let private isRelatedError n = function
    | ValuesNotEqual v -> v = n
    | _ -> false

let tryGetFieldError txt (res:ValidationResult) =
    res.Errors
    |> List.tryFind (fun e -> e.Field = txt)
    |> Option.orElse (res.Errors |> List.tryFind (fun x -> isRelatedError txt x.ErrorType))