module Yobo.Shared.Communication

open Yobo.Shared.Validation

type AuthenticationError =
    | InvalidLoginOrPassword
    
module AuthenticationError =
    let explain = function
        | InvalidLoginOrPassword -> "Nesprávně vyplněný email nebo heslo."

type ServerError =
    | Exception of string
    | Validation of ValidationError list
    | Authentication of AuthenticationError

type ServerResult<'a> = Result<'a, ServerError>

module ServerResult =
    let ofValidation (validationFn:'a -> ValidationError list) (value:'a) : ServerResult<'a> =
        match value |> validationFn with
        | [] -> Ok value
        | errs -> errs |> Validation |> Error

type ServerResponse<'a> = Async<ServerResult<'a>>