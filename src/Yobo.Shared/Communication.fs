module Yobo.Shared.Communication

open Yobo.Shared.Validation

type ServerError =
    | Exception of string
    | Validation of ValidationError list

type ServerResult<'a> = Result<'a, ServerError>

module ServerResult =
    let ofValidation (validationFn:'a -> ValidationError list) (value:'a) : ServerResult<'a> =
        match value |> validationFn with
        | [] -> Ok value
        | errs -> errs |> Validation |> Error

type ServerResponse<'a> = Async<ServerResult<'a>>