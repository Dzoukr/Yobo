namespace Yobo.Core.Lessons

open System

module CmdArgs = 

    type CreateLesson = {
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

    type CancelLesson = {
        Id : Guid
    }

    type CreateWorkshop = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
    }

    type DeleteWorkshop = {
        Id : Guid
    }

    type AddCredits = {
        UserId : Guid
        Credits : int
        Expiration : DateTimeOffset
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
    | LessonCreated of CmdArgs.CreateLesson
    | ReservationAdded of CmdArgs.AddReservation
    | ReservationCancelled of CmdArgs.CancelReservation
    | LessonCancelled of CmdArgs.CancelLesson
    | CreditsAdded of CmdArgs.AddCredits
    | CreditsWithdrawn of CmdArgs.WithdrawCredits
    | CreditsRefunded of CmdArgs.RefundCredits
    | CashReservationsBlocked of CmdArgs.BlockCashReservations
    | CashReservationsUnblocked of CmdArgs.UnblockCashReservations
    | WorkshopCreated of CmdArgs.CreateWorkshop
    | WorkshopDeleted of CmdArgs.DeleteWorkshop