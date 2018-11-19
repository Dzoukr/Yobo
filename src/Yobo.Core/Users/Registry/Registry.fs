namespace Yobo.Core.Users.Registry

open System

module CmdArgs = 
    type Add = {
        Email: string
        UserId : Guid
    }
    type Remove = {
        Email: string
        UserId : Guid
    }

type Command =
    | Add of CmdArgs.Add
    | Remove of CmdArgs.Remove

type Event = 
    | Added of CmdArgs.Add
    | Removed of CmdArgs.Remove

type State = {
    Emails : (Guid * string) list
}
with
    static member Init = {
        Emails = []
    }