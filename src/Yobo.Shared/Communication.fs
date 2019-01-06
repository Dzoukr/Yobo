module Yobo.Shared.Communication

open System
open Yobo.Shared.Validation
open Yobo.Shared.Domain
open Yobo.Shared.Auth

type ServerError =
    | ValidationError of ValidationError list
    | DomainError of DomainError
    | AuthError of AuthError
    | Exception of string

type ServerResponse<'a> = Async<Result<'a, ServerError>>

type SecuredParam<'a> = {
    Token : string
    Param: 'a
}

module FrontendRoutes =
    let activateAccount : PrintfFormat<(Guid -> string),unit,string,string,Guid> = "/accountActivation/%O"