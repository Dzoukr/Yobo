module Yobo.Shared.Auth.Communication

open System
open Yobo.Shared.Domain
open Yobo.Shared.Communication
open Yobo.Shared.Auth.Domain

let routeBuilder _ m = sprintf "/api/auth/%s" m

type API = {
    Register : NewAccount -> ServerResponse<Guid>
    ActivateAccount : Guid -> ServerResponse<Guid>
    GetToken : Login -> ServerResponse<string>
    RefreshToken : string -> ServerResponse<string>
    GetUserByToken : string -> ServerResponse<User>
    ResendActivation : Guid -> ServerResponse<Guid>
    InitiatePasswordReset : string -> ServerResponse<unit>
} 