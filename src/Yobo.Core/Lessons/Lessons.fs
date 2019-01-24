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

    type Reopen = {
        Id : Guid
    }

type Command =
    | Create of CmdArgs.Create
    | AddReservation of CmdArgs.AddReservation
    | CancelReservation of CmdArgs.CancelReservation
    | Cancel of CmdArgs.Cancel

type Event =
    | Created of CmdArgs.Create
    | ReservationAdded of CmdArgs.AddReservation
    | ReservationCancelled of CmdArgs.CancelReservation
    | Cancelled of CmdArgs.Cancel
    | Reopened of CmdArgs.Reopen

type State = {
    Id : Guid
    Reservations : (Guid * int * bool) list
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    IsCancelled : bool
}
with
    static member Init = {
        Id = Guid.Empty
        Reservations = []
        StartDate = DateTimeOffset.MinValue
        EndDate = DateTimeOffset.MinValue
        IsCancelled = false
    }