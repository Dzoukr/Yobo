module Yobo.Shared.Registration.Communication

open System
open Yobo.Shared.Communication
open Yobo.Shared.Registration.Domain

let routeBuilder _ m = sprintf "/api/register/%s" m

type API = {
    Register : Account -> ServerResponse<Guid>
    ActivateAccount : Guid -> ServerResponse<Guid>
} 