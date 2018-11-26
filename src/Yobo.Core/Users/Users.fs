namespace Yobo.Core.Users

open System

module CmdArgs = 
    type Register = {
        Id : Guid
        ActivationKey : Guid
        PasswordHash: string
        FirstName: string
        LastName: string
        Email: string
    }

type Command =
    | Register of CmdArgs.Register

type Event = 
    | Registered of CmdArgs.Register

type State = {
    Id : Guid
}
with
    static member Init = { Id = Guid.Empty }