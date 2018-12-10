namespace Yobo.Core.Users

open System

module CmdArgs = 
    type Register = {
        Id : Guid
        ActivationKey : Guid
        PasswordHash: string
        FirstName: string
        LastName: string
        Email: string
    }

    type Activate = {
        Id : Guid
        ActivationKey: Guid
    }

    type RegenerateActivationKey = {
        Id : Guid
        ActivationKey: Guid
    }

type Command =
    | Register of CmdArgs.Register
    | RegenerateActivationKey of CmdArgs.RegenerateActivationKey
    | Activate of CmdArgs.Activate

type Event = 
    | Registered of CmdArgs.Register
    | ActivationKeyRegenerated of CmdArgs.RegenerateActivationKey
    | Activated of CmdArgs.Activate

type State = {
    Id : Guid
    IsActivated : bool
    ActivationKey : Guid
}
with
    static member Init = { Id = Guid.Empty; IsActivated = false; ActivationKey = Guid.Empty }