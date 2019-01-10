module Yobo.Core.Metadata

open System

type Metadata = {
    UserId : Guid option
    CreatedUtc : DateTime
}
with
    static member Create userId = {
        UserId = Some userId
        CreatedUtc = DateTime.UtcNow
    }
    static member CreateAnonymous() = {
        UserId = None
        CreatedUtc = DateTime.UtcNow
    }