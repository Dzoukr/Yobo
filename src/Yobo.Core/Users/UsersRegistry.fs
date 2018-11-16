namespace Yobo.Core.UsersRegistry

open System

module CmdArgs = 
    type Add = {
        Email: string
    }
    type Remove = {
        Email: string
    }

type Command =
    | Add of CmdArgs.Add
    | Remove of CmdArgs.Remove

type Event = 
    | Added of CmdArgs.Add
    | Removed of CmdArgs.Remove

type State = {
    Emails : string list
}

type CompensatedFlow<'command, 'event> = {
    Command : 'command
    OnError : 'event list
}

type CommandFlow<'command, 'compCmd, 'compEvent> =
    | Direct of 'command
    | Compensated of cmd:'command * CompensatedFlow<'compCmd, 'compEvent>