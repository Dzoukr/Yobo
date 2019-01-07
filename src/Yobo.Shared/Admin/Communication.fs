module Yobo.Shared.Admin.Communication

open System
open Domain
open Yobo.Shared.Communication

let routeBuilder _ m = sprintf "/api/admin/%s" m

type API = {
    GetAllUsers : SecuredParam<unit> -> ServerResponse<User list>
} 