module Yobo.Core.Metadata

open System

type Metadata = {
    UserId : Guid option
    Created : DateTimeOffset
}
with
    static member Create userId = {
        UserId = Some userId
        Created = DateTimeOffset.Now
    }
    static member CreateAnonymous() = {
        UserId = None
        Created = DateTimeOffset.Now
    }