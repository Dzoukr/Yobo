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

module Routes =
    let register = "/api/register"
    let login = "/api/login"
    let activateAccount : PrintfFormat<(Guid -> string),unit,string,string,Guid> = "/api/activateAccount/%O"

module FrontendRoutes =
    let activateAccount : PrintfFormat<(Guid -> string),unit,string,string,Guid> = "/accountActivation/%O"