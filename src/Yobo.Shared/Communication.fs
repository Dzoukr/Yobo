module Yobo.Shared.Communication

open System
open Yobo.Shared.Validation
open Yobo.Shared.Domain

type ServerError =
    | ValidationError of ValidationError list
    | DomainError of DomainError
    | Exception of string

module Routes =
    let register = "/register"
    let AccountActivation : PrintfFormat<(Guid -> string),unit,string,string,Guid> = "/activateAccount/%O"

