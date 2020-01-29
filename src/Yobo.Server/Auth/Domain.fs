module Yobo.Server.Auth.Domain

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
        ActivationKey : Guid
    }

    type RegenerateActivationKey = {
        Id : Guid
        ActivationKey : Guid
    }

    type InitiatePasswordReset = {
        Email : string
        PasswordResetKey : Guid
    }

    type CompleteResetPassword = {
        PasswordResetKey : Guid
        PasswordHash : string
    }

    type SubscribeToNewsletters = {
        Id : Guid
    }

module EventArgs =
    type Activated = {
        Id : Guid
    }
    type PasswordResetInitiated = {
        Id : Guid
        PasswordResetKey : Guid
    }
    type PasswordResetComplete = {
        Id : Guid
        PasswordResetKey : Guid
        PasswordHash : string
    }
    
type Event = 
    | Registered of CmdArgs.Register
    | ActivationKeyRegenerated of CmdArgs.RegenerateActivationKey
    | Activated of EventArgs.Activated
    | PasswordResetInitiated of EventArgs.PasswordResetInitiated
    | PasswordResetComplete of EventArgs.PasswordResetComplete
    | SubscribedToNewsletters of CmdArgs.SubscribeToNewsletters    