module Yobo.API.Users.ArgsBuilder

open System
open Yobo.Core
open Yobo.Core.Users
open Yobo.Shared.Users
open Yobo.Shared.Users.Domain
open Yobo.Shared.Communication

let buildRegister getHash =
    ArgsBuilder.build (fun (acc:NewAccount) ->
        ({
            Id = Guid.NewGuid()
            ActivationKey = Guid.NewGuid()
            PasswordHash = acc.Password |> getHash
            FirstName = acc.FirstName
            LastName = acc.LastName
            Email = acc.Email.ToLower()
        } : CmdArgs.Register)
    ) Validation.validateAccount
    >> Result.mapError ServerError.ValidationError