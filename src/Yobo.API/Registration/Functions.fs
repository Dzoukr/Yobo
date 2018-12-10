module Yobo.API.Registration.Functions

open Yobo.Shared.Registration.Domain
open FSharp.Rop
open Yobo.Core
open Yobo.Core.Users
open System

let register cmdHandler createHashFn (acc:Account) =
    result {
        let! args = acc |> ArgsBuilder.buildRegister createHashFn
        let! _ = args |> Command.Register |> CoreCommand.Users |> cmdHandler
        return args.Id
    }

let activateAccount cmdHandler getUserByActivationKey (activationKey:Guid) =
    result {
        let! (user : ReadQueries.User) = getUserByActivationKey activationKey
        let! _ = ({ Id = user.Id; ActivationKey = activationKey } : CmdArgs.Activate) |> Command.Activate |> CoreCommand.Users |> cmdHandler
        return user.Id
    }