module Yobo.Server.Core.Domain

open System

module CmdArgs =
    
    type AddCredits = {
        UserId : Guid
        Credits : int
        Expiration : DateTimeOffset
    }
    
//    type CreateLesson = {
//        Id : Guid
//        StartDate : DateTimeOffset
//        EndDate : DateTimeOffset
//        Name : string
//        Description : string
//    }
//
//    type AddReservation = {
//        Id : Guid
//        UserId : Guid
//        Count : int
//        UseCredits : bool
//    }
//
//    type CancelReservation = {
//        Id : Guid
//        UserId : Guid
//    }
//
//    type CancelLesson = {
//        Id : Guid
//    }
//
//    type CreateWorkshop = {
//        Id : Guid
//        StartDate : DateTimeOffset
//        EndDate : DateTimeOffset
//        Name : string
//        Description : string
//    }
//
//    type DeleteWorkshop = {
//        Id : Guid
//    }
//
//
//    type WithdrawCredits = {
//        UserId : Guid
//        Amount : int
//        LessonId : Guid
//    }
//
//    type RefundCredits = {
//        UserId : Guid
//        Amount : int
//        LessonId : Guid
//    }
//
//    type BlockCashReservations = {
//        UserId : Guid
//        Expires : DateTimeOffset
//        LessonId : Guid
//    }
//
//    type UnblockCashReservations = {
//        UserId : Guid
//    }
//
//    type ExtendExpiration = {
//        UserId : Guid
//        Expiration : DateTimeOffset
//    }
//
//    type DeleteLesson = {
//        Id : Guid
//    }
//
//    type UpdateLesson = {
//        Id : Guid
//        StartDate : DateTimeOffset
//        EndDate : DateTimeOffset
//        Name : string
//        Description : string
//    }


type Event =
    | CreditsAdded of CmdArgs.AddCredits
    
//    | LessonCreated of CmdArgs.CreateLesson
//    | ReservationAdded of CmdArgs.AddReservation
//    | ReservationCancelled of CmdArgs.CancelReservation
//    | LessonCancelled of CmdArgs.CancelLesson
//    | CreditsWithdrawn of CmdArgs.WithdrawCredits
//    | CreditsRefunded of CmdArgs.RefundCredits
//    | CashReservationsBlocked of CmdArgs.BlockCashReservations
//    | CashReservationsUnblocked of CmdArgs.UnblockCashReservations
//    | WorkshopCreated of CmdArgs.CreateWorkshop
//    | WorkshopDeleted of CmdArgs.DeleteWorkshop
//    | ExpirationExtended of CmdArgs.ExtendExpiration
//    | LessonDeleted of CmdArgs.DeleteLesson
//    | LessonUpdated of CmdArgs.UpdateLesson