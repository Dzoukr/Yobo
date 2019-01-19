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
    with
        member x.Explain() =
            match x with
            | ValidationError _ -> "Došlo k chybě správnosti dat. Prosím zkontrolujte formulář."
            | DomainError e -> e.Explain()
            | AuthError e -> e.Explain()
            | Exception ex -> sprintf "Došlo k chybě : %s" ex

type ServerResult<'a> = Result<'a, ServerError>
type ServerResponse<'a> = Async<ServerResult<'a>>

type SecuredParam<'a> = {
    Token : string
    Param: 'a
}

module FrontendRoutes =
    let activateAccount : PrintfFormat<(Guid -> string),unit,string,string,Guid> = "/accountActivation/%O"