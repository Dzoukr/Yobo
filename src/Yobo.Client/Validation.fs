module Yobo.Client.Validation

open Yobo.Shared.Validation

let tryGetFieldError txt (res:ValidationResult) =
    let findFn = function
        | IsEmpty t -> txt = t
        | MustBeLongerThan (t,_) -> txt = t
        | MustBeBiggerThan (t,_) -> txt = t
        | MustBeAfter (t,_) -> txt = t
        | ValuesNotEqual (_, t2) -> txt = t2
        | IsNotValidEmail t -> txt = t
    res.Errors |> List.tryFind findFn