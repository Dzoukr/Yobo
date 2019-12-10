module Yobo.Shared.Communication

open Yobo.Shared.Validation

type ServerError =
    | Exception of string
    | Validation of ValidationError list

type ServerResponse<'a> = Async<Result<'a, ServerError>>