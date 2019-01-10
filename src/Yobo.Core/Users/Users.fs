namespace Yobo.Core.Users

open System

module CmdArgs = 
    type Register = {
        Id : Guid
        ActivationKey : Guid
        PasswordHash : string
        FirstName : string
        LastName : string
        Email : string
    }

    type Activate = {
        Id : Guid
        ActivationKey : Guid
    }

    type RegenerateActivationKey = {
        Id : Guid
        ActivationKey : Guid
    }

    type AddCredits = {
        Id : Guid
        Credits : int
        ExpirationUtc : DateTime
    }

type Command =
    | Register of CmdArgs.Register
    | RegenerateActivationKey of CmdArgs.RegenerateActivationKey
    | Activate of CmdArgs.Activate
    | AddCredits of CmdArgs.AddCredits

type Event = 
    | Registered of CmdArgs.Register
    | ActivationKeyRegenerated of CmdArgs.RegenerateActivationKey
    | Activated of CmdArgs.Activate
    | CreditsAdded of CmdArgs.AddCredits

type State = {
    Id : Guid
    IsActivated : bool
    ActivationKey : Guid
    Credits : int
    CreditsExpirationUtc : DateTime option
}
with
    static member Init = {
        Id = Guid.Empty
        IsActivated = false
        ActivationKey = Guid.Empty
        Credits = 0
        CreditsExpirationUtc = None
    }