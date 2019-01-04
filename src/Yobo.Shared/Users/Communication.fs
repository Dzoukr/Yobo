module Yobo.Shared.Users.Communication

open System
open Yobo.Shared.Communication
open Yobo.Shared.Users.Domain

let routeBuilder _ m = sprintf "/api/users/%s" m

type API = {
    Register : NewAccount -> ServerResponse<Guid>
    ActivateAccount : Guid -> ServerResponse<Guid>
    Login : Login -> ServerResponse<string>
    GetUser : string -> ServerResponse<User>
    ResendActivation: Guid -> ServerResponse<Guid>
} 