module Yobo.Shared.Communication

open Yobo.Shared.Validation

type ServerError =
    | Exception of string
    | Validation of ValidationError list

type ServerResult<'a> = Result<'a, ServerError>
type ServerResponse<'a> = Async<ServerResult<'a>>