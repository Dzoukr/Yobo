namespace Yobo.Core.Auth

open System

module CmdArgs = 
    type Register = {
        Id : Guid
        ActivationKey : Guid
        PasswordHash : string
        FirstName : string
        LastName : string
        Email : string
        Newsletters : bool
    }

    type Activate = {
        Id : Guid
        ActivationKey : Guid
    }

    type RegenerateActivationKey = {
        Id : Guid
        ActivationKey : Guid
    }

    type InitiatePasswordReset = {
        Id : Guid
        PasswordResetKey : Guid
    }

    type ResetPassword = {
        Id : Guid
        PasswordResetKey : Guid
        PasswordHash : string
    }

type Event = 
    | Registered of CmdArgs.Register
    | ActivationKeyRegenerated of CmdArgs.RegenerateActivationKey
    | Activated of CmdArgs.Activate
    | PasswordResetInitiated of CmdArgs.InitiatePasswordReset
    | PasswordReset of CmdArgs.ResetPassword