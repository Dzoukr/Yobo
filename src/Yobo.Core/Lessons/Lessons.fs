namespace Yobo.Core.Lessons

open System

module CmdArgs = 

    type Create = {
        Id : Guid
        StartDateUtc : DateTime
        EndDateUtc : DateTime
        Name : string
        Description : string
    }

type Command =
    | Create of CmdArgs.Create

type Event =
    | Created of CmdArgs.Create

type State = {
    Id : Guid
}
with
    static member Init = {
        Id = Guid.Empty
    }