module Yobo.Shared.Domain

open Yobo.Shared.Validation

type AuthenticationError =
    | InvalidLoginOrPassword
    | InvalidOrExpiredToken
    | EmailAlreadyRegistered
    
module AuthenticationError =
    let explain = function
        | InvalidLoginOrPassword -> "Nesprávně vyplněný email nebo heslo."
        | InvalidOrExpiredToken -> "Token není validní nebo již vypršela jeho platnost."
        | EmailAlreadyRegistered -> "Tento email je již v systému registrován."

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