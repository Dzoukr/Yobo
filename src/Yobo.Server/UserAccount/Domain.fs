module Yobo.Server.UserAccount.Domain

open System

module Queries =
    type UserAccount = {
        Id : Guid
        FirstName : string
        LastName : string
    }