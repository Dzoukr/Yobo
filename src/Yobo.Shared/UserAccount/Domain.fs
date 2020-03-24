module Yobo.Shared.UserAccount.Domain

open System

module Queries =
    type UserAccount = {
        Id : Guid
        FirstName : string
        LastName : string
        IsActivated : bool
        Credits : int
        CreditsExpiration : DateTimeOffset option
    }