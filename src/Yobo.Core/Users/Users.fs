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

    type InitiatePasswordReset = {
        Id : Guid
        PasswordResetKey : Guid
    }

    type ResetPassword = {
        Id : Guid
        PasswordResetKey : Guid
        PasswordHash : string
    }

    type AddCredits = {
        Id : Guid
        Credits : int
        Expiration : DateTimeOffset
    }

    type WithdrawCredits = {
        Id : Guid
        Amount : int
        LessonId : Guid
    }

    type RefundCredits = {
        Id : Guid
        Amount : int
        LessonId : Guid
    }

    type BlockCashReservations = {
        Id : Guid
        Expires : DateTimeOffset
        LessonId : Guid
    }

    type UnblockCashReservations = {
        Id : Guid
    }

type Command =
    | Register of CmdArgs.Register
    | RegenerateActivationKey of CmdArgs.RegenerateActivationKey
    | Activate of CmdArgs.Activate
    | InitiatePasswordReset of CmdArgs.InitiatePasswordReset
    | ResetPassword of CmdArgs.ResetPassword
    | AddCredits of CmdArgs.AddCredits
    | WithdrawCredits of CmdArgs.WithdrawCredits
    | RefundCredits of CmdArgs.RefundCredits
    | BlockCashReservations of CmdArgs.BlockCashReservations
    | UnblockCashReservations of CmdArgs.UnblockCashReservations

type Event = 
    | Registered of CmdArgs.Register
    | ActivationKeyRegenerated of CmdArgs.RegenerateActivationKey
    | Activated of CmdArgs.Activate
    | PasswordResetInitiated of CmdArgs.InitiatePasswordReset
    | PasswordReset of CmdArgs.ResetPassword
    | CreditsAdded of CmdArgs.AddCredits
    | CreditsWithdrawn of CmdArgs.WithdrawCredits
    | CreditsRefunded of CmdArgs.RefundCredits
    | CashReservationsBlocked of CmdArgs.BlockCashReservations
    | CashReservationsUnblocked of CmdArgs.UnblockCashReservations

type State = {
    Id : Guid
    IsActivated : bool
    ActivationKey : Guid
    PasswordResetKey : Guid option
    Credits : int
    CreditsExpiration : DateTimeOffset option
    CashReservationsBlockedUntil : DateTimeOffset option
    LastCashBlocking : (Guid * DateTimeOffset) option
}
with
    static member Init = {
        Id = Guid.Empty
        IsActivated = false
        ActivationKey = Guid.Empty
        PasswordResetKey = None
        Credits = 0
        CreditsExpiration = None
        CashReservationsBlockedUntil = None
        LastCashBlocking = None
    }