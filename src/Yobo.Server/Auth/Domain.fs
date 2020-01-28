module Yobo.Server.Auth.Domain

open System

module Queries =
    type AuthUserView = {
        Id : Guid
        Email : string
        PasswordHash : string
        FirstName : string
        LastName : string
    }

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
        Id : Guid
        PasswordResetKey : Guid
    }

    type CompleteResetPassword = {
        Id : Guid
        PasswordResetKey : Guid
        PasswordHash : string
    }

    type SubscribeToNewsletters = {
        Id : Guid
    }

module EventArgs =
    type Registered = {
        Id : Guid
        ActivationKey : Guid
        PasswordHash : string
        FirstName : string
        LastName : string
        Email : string
        Newsletters : bool
    }
    
type Event = 
    | Registered of CmdArgs.Register
    | ActivationKeyRegenerated of CmdArgs.RegenerateActivationKey
    | Activated of CmdArgs.Activate
    | PasswordResetInitiated of CmdArgs.InitiatePasswordReset
    | PasswordResetComplete of CmdArgs.CompleteResetPassword
    | SubscribedToNewsletters of CmdArgs.SubscribeToNewsletters    