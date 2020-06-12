module Yobo.Server.Core.Domain

open System

module CmdArgs =
    
    type AddCredits = {
        UserId : Guid
        Credits : int
        Expiration : DateTimeOffset
    }
    
    type SetExpiration = {
        UserId : Guid
        Expiration : DateTimeOffset
    }
    
    type CreateLesson = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
        Capacity : int
        CancellableBeforeStart : TimeSpan
    }
    
    type CreateWorkshop = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
    }
    
    type ChangeLessonDescription = {
        Id : Guid
        Name : string
        Description : string
    }
    
    type CancelLesson = {
        Id : Guid
    }
    
    type RefundCredit = {
        UserId : Guid
    }
    
    type UnblockCashReservations = {
        UserId : Guid
    }
    
    type CancelLessonReservation = {
        UserId : Guid
        LessonId : Guid
    }
    
    type DeleteLesson = {
        Id : Guid
    }
    
    type DeleteWorkshop = {
        Id : Guid
    }
    
    type AddLessonReservation = {
        LessonId : Guid
        UserId : Guid
        UseCredits : bool
    }
    
    type BlockCashReservations = {
        UserId : Guid
        Expires : DateTimeOffset
        LessonId : Guid
    }
    
    type WithdrawCredit = {
        UserId : Guid
        LessonId : Guid
    }

type Event =
    | CreditsAdded of CmdArgs.AddCredits
    | ExpirationSet of CmdArgs.SetExpiration
    | LessonCreated of CmdArgs.CreateLesson
    | WorkshopCreated of CmdArgs.CreateWorkshop
    | LessonDescriptionChanged of CmdArgs.ChangeLessonDescription
    | LessonCancelled of CmdArgs.CancelLesson
    | CreditRefunded of CmdArgs.RefundCredit
    | CashReservationsUnblocked of CmdArgs.UnblockCashReservations
    | LessonReservationCancelled of CmdArgs.CancelLessonReservation
    | LessonDeleted of CmdArgs.DeleteLesson
    | WorkshopDeleted of CmdArgs.DeleteWorkshop
    | LessonReservationAdded of CmdArgs.AddLessonReservation
    | CreditWithdrawn of CmdArgs.WithdrawCredit
    | CashReservationsBlocked of CmdArgs.BlockCashReservations
