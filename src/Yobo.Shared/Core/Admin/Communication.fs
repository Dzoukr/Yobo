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
    
    module AddCredits =
        let init = {
            UserId = Guid.Empty
            Credits = 0
            Expiration = DateTimeOffset.MinValue
        }
    
    type SetExpiration = {
        UserId : Guid
        Expiration : DateTimeOffset
    }
    
    module SetExpiration =
        let init = {
            UserId = Guid.Empty
            Expiration = DateTimeOffset.MinValue
        }
        
    
type AdminService = {
    GetAllUsers : unit -> Async<Queries.User list>
    AddCredits : Request.AddCredits -> Async<unit>
    SetExpiration : Request.SetExpiration -> Async<unit>
}
with
    static member RouteBuilder _ m = sprintf "/api/admin/%s" m    