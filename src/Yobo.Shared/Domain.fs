module Yobo.Shared.Domain

open System

type DomainError =
    | ItemAlreadyExists of Text.TextValue
    | ItemDoesNotExist of Text.TextValue
    | UserAlreadyActivated
    | ActivationKeyDoesNotMatch

type User = {
    Id : Guid
    Email : string
    FirstName : string
    LastName : string
    IsAdmin : bool
}