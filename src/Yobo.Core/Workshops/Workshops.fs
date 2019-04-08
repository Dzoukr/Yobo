namespace Yobo.Core.Workshops

open System

module CmdArgs = 

    type Create = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
    }

    type Delete = {
        Id : Guid
    }
     
type Command =
    | Create of CmdArgs.Create
    | Delete of CmdArgs.Delete

type Event =
    | Created of CmdArgs.Create
    | Deleted of CmdArgs.Delete

type State = {
    Id : Guid
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    IsDeleted : bool
}
with
    static member Init = {
        Id = Guid.Empty
        StartDate = DateTimeOffset.MinValue
        EndDate = DateTimeOffset.MinValue
        IsDeleted = false
    }