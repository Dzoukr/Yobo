module Yobo.Core.Users.ArgsBuilder

open System
open Yobo.Core
open Yobo.Core.Users.CmdArgs
open Yobo.Shared.Login.Register
open Yobo.Shared.Login.Register.Domain

// TODO : Move it to Server part

let buildRegister getHash =
    ArgsBuilder.build (fun (acc:Account) ->
        {
            Id = Guid.NewGuid()
            ConfirmationKey = Guid.NewGuid()
            PasswordHash = acc.Password |> getHash
            FirstName = acc.FirstName
            LastName = acc.LastName
            Email = acc.Email.ToLower()
        } : CmdArgs.Register
    ) Validation.validateAccount