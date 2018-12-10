module Yobo.Shared.Login.Communication

open System
open Yobo.Shared.Communication
open Yobo.Shared.Login.Domain

let routeBuilder _ m = sprintf "/api/login/%s" m

type API = {
    Login : Login -> ServerResponse<User>
    ResendActivation: Guid -> ServerResponse<Guid>
} 