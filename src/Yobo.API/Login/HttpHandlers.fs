module Yobo.API.Login.HttpHandlers

open Giraffe

open Yobo.Shared.Register.Domain
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

let validateAccount cmdHandler getUserByConfId (confirmationId:Guid) =
    result {
        let! user = getUserByConfId confirmationId
        return ()
    }