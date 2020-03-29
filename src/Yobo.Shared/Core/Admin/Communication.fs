module Yobo.Shared.Core.Admin.Communication

open System
open Domain
    
[<RequireQualifiedAccess>]
module Request =
    type AddCredits = {
        UserId : Guid
        Credits : int
        Expiration : DateTimeOffset
    }

type AdminService = {
    GetAllUsers : unit -> Async<Queries.User list>
    AddCredits : Request.AddCredits -> Async<unit>
}
with
    static member RouteBuilder _ m = sprintf "/api/admin/%s" m    