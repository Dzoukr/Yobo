module Yobo.Shared.Core.UserAccount.Domain

open System

module Queries =
    type UserAccount = {
        Id : Guid
        Email : string
        FirstName : string
        LastName : string
        IsActivated : bool
        Credits : int
        CreditsExpiration : DateTimeOffset option
        IsAdmin : bool
    }