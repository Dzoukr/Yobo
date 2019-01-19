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

type Command =
    | Create of CmdArgs.Create
    | AddReservation of CmdArgs.AddReservation
    | CancelReservation of CmdArgs.CancelReservation

type Event =
    | Created of CmdArgs.Create
    | ReservationAdded of CmdArgs.AddReservation
    | ReservationCancelled of CmdArgs.CancelReservation

type State = {
    Id : Guid
    Reservations : (Guid * int * bool) list
    EndDate : DateTimeOffset
}
with
    static member Init = {
        Id = Guid.Empty
        Reservations = []
        EndDate = DateTimeOffset.MinValue
    }