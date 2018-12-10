module Yobo.API.Login.Functions

open System
open Yobo.Shared.Login.Domain
open FSharp.Rop
open Yobo.Core.Users
open Yobo.Core

let private mapToUser (u:Yobo.Core.Users.ReadQueries.User) =
    {
        Id = u.Id
        Email = u.Email
        FirstName = u.FirstName
        LastName = u.LastName
    }

let login loginFn (acc:Login) =
    result {
        let! user = loginFn acc.Email acc.Password
        return user |> mapToUser
    }

let resendActivation cmdHandler (userId:Guid) =
    result {
        let newKey = Guid.NewGuid()
        let! _ = ({ Id = userId; ActivationKey = newKey } : CmdArgs.RegenerateActivationKey) |> Command.RegenerateActivationKey |> CoreCommand.Users |> cmdHandler
        return userId
    }