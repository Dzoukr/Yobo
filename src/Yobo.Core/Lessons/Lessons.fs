namespace Yobo.Core.Lessons

open System

module CmdArgs = 

    type Create = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
    }

    type AddReservation = {
        Id : Guid
        UserId : Guid
        Count : int
        UseCredits : bool
    }

    type CancelReservation = {
        Id : Guid
        UserId : Guid
    }

    type Cancel = {
        Id : Guid
    }

    type WithdrawCredits = {
        UserId : Guid
        Amount : int
        LessonId : Guid
    }

    type RefundCredits = {
        UserId : Guid
        Amount : int
        LessonId : Guid
    }

    type BlockCashReservations = {
        UserId : Guid
        Expires : DateTimeOffset
        LessonId : Guid
    }

    type UnblockCashReservations = {
        UserId : Guid
    }

type Event =
    | Created of CmdArgs.Create
    | ReservationAdded of CmdArgs.AddReservation
    | ReservationCancelled of CmdArgs.CancelReservation
    | Cancelled of CmdArgs.Cancel
    | CreditsWithdrawn of CmdArgs.WithdrawCredits
    | CreditsRefunded of CmdArgs.RefundCredits
    | CashReservationsBlocked of CmdArgs.BlockCashReservations
    | CashReservationsUnblocked of CmdArgs.UnblockCashReservations