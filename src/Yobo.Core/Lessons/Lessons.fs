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
    }

type Command =
    | Create of CmdArgs.Create
    | AddReservation of CmdArgs.AddReservation

type Event =
    | Created of CmdArgs.Create
    | ReservationAdded of CmdArgs.AddReservation

type State = {
    Id : Guid
    Reservations : (Guid * int) list
}
with
    static member Init = {
        Id = Guid.Empty
        Reservations = []
    }