module Yobo.Core.Users.ArgsBuilder

open System
open Yobo.Core
open Yobo.Core.Users.CmdArgs
open Yobo.Shared.Login.Register
open Yobo.Shared.Login.Register.Domain

// TODO : Move it to Server part

let buildCreate getHash =
    ArgsBuilder.build (fun (acc:Account) ->
        {
            Id = Guid.NewGuid()
            ConfirmationKey = Guid.NewGuid()
            PasswordHash = acc.Password |> getHash
            FirstName = acc.FirstName
            LastName = acc.LastName
            Email = acc.Email
        } : CmdArgs.Create
    ) Validation.validateAccount