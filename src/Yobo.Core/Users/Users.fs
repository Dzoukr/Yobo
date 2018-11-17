namespace Yobo.Core.Users

open System

module CmdArgs = 
    type Create = {
        Id : Guid
        ConfirmationKey : Guid
        PasswordHash: string
        FirstName: string
        LastName: string
        Email: string
    }

type Command =
    | Create of CmdArgs.Create

type Event = 
    | Created of CmdArgs.Create

type State = {
    Id : Guid
}
with
    static member Init = { Id = Guid.Empty }