module Yobo.Shared.Admin.Domain

open System

type User = {
    Id : Guid
    Email : string
    FirstName : string
    LastName : string
    ActivatedUtc : DateTime option
    Credits : int
    CreditsExpirationUtc : DateTime option
}

type AddCredits = {
    UserId : Guid
    Credits : int
    ExpirationUtc : DateTime
}