module Yobo.API.Auth.Functions

open System
open Yobo.Shared.Auth.Domain
open FSharp.Rop
open Yobo.Core.Users
open Yobo.Core.CQRS
open Yobo.Shared.Auth
open System.Security.Claims
open Yobo.Shared.Communication
open Yobo.Shared.Domain

module ArgsBuilder =
    open Yobo.API

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

    let buildPasswordReset userId key getHash =
        ArgsBuilder.build (fun (pr:PasswordReset) ->
            ({
                Id = userId
                PasswordResetKey = key
                PasswordHash = pr.Password |> getHash
            } : CmdArgs.ResetPassword)
        ) Validation.validatePasswordReset
        >> Result.mapError ServerError.ValidationError

let claimsToUser getUserById (claims:seq<Claim>) =
    let find key = claims |> Seq.find (fun x -> x.Type = key) |> (fun x -> x.Value)
    let id = find "Id" |> Guid
    id |> getUserById
    
let userToClaims (u:User) =
    seq [
        Claim("Id", u.Id.ToString())
        Claim("Email", u.Email)
        Claim("FirstName", u.FirstName)
        Claim("LastName", u.LastName)
    ]

let getToken loginFn tokenCreator (acc:Login) =
    result {
        let! user = loginFn acc.Email acc.Password
        return user |> userToClaims |> tokenCreator
    }
let refreshToken validateFn tokenCreator token =
    match token |> validateFn with
    | Some claims -> claims |> tokenCreator |> Ok
    | None -> AuthError.InvalidOrExpiredToken |> ServerError.AuthError |> Error

let resendActivation cmdHandler (userId:Guid) =
    result {
        let newKey = Guid.NewGuid()
        let! _ = ({ Id = userId; ActivationKey = newKey } : CmdArgs.RegenerateActivationKey) |> Command.RegenerateActivationKey |> CoreCommand.Users |> cmdHandler
        return userId
    }

let getUser getById validateFn token =
    match token |> validateFn with
    | Some claims -> claims |> claimsToUser getById
    | None -> AuthError.InvalidOrExpiredToken |> ServerError.AuthError |> Error

let register cmdHandler createHashFn (acc:NewAccount) =
    result {
        let! args = acc |> ArgsBuilder.buildRegister createHashFn
        let! _ = args |> Command.Register |> CoreCommand.Users |> cmdHandler
        return args.Id
    }

let activateAccount cmdHandler getUserByActivationKey (activationKey:Guid) =
    result {
        let! (user : User) = getUserByActivationKey activationKey
        let! _ = ({ Id = user.Id; ActivationKey = activationKey } : CmdArgs.Activate) |> Command.Activate |> CoreCommand.Users |> cmdHandler
        return user.Id
    }

let initiatePasswordReset cmdHandler getUserByEmail (email:string) =
    result {
        let! (user : User) = getUserByEmail email
        let! _ = ({ Id = user.Id; PasswordResetKey = Guid.NewGuid() } : CmdArgs.InitiatePasswordReset) |> Command.InitiatePasswordReset |> CoreCommand.Users |> cmdHandler
        return ()
    }

let resetPassword cmdHandler createHashFn getByPwdResetKey (key:Guid,pwdReset:PasswordReset) =
    result {
        let! (user : User) = key |> getByPwdResetKey
        let! args = pwdReset |> ArgsBuilder.buildPasswordReset user.Id key createHashFn
        let! _ = args |> Command.ResetPassword |> CoreCommand.Users |> cmdHandler
        return ()
    }