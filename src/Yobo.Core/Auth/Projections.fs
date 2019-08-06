module Yobo.Core.Auth.Projections

open System

type ExistingUser = {
    Id : Guid
    Email : string
    IsActivated : bool
    ActivationKey : Guid
    PasswordResetKey : Guid option
}