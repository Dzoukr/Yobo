module Yobo.FunctionApp.Auth.Functions

open System
open Yobo.Shared.Auth.Domain
open FSharp.Rop
open Yobo.Core.Auth
open Yobo.Shared.Auth
open System.Security.Claims
open Yobo.Shared.Communication
open Yobo.Shared.Domain

module ArgsBuilder =
    open Yobo.FunctionApp

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

let resendActivation getProjection cmdHandler (userId:Guid) =
    result {
        let! proj = getProjection userId |> Result.ofOption (DomainError.ItemDoesNotExist "Id" |> ServerError.DomainError)
        let newKey = Guid.NewGuid()
        let! _ = ({ Id = userId; ActivationKey = newKey } : CmdArgs.RegenerateActivationKey) |> cmdHandler proj
        return userId
    }

let getUser getById validateFn token =
    match token |> validateFn with
    | Some claims -> claims |> claimsToUser getById
    | None -> AuthError.InvalidOrExpiredToken |> ServerError.AuthError |> Error

let register getProjection cmdHandler createHashFn (acc:NewAccount) =
    result {
        let proj = getProjection ()
        let! args = acc |> ArgsBuilder.buildRegister createHashFn
        let! _ = args |> cmdHandler proj
        return args.Id
    }

let activateAccount getProjection cmdHandler (activationKey:Guid) =
    result {
        let! (user : Yobo.Core.Auth.Projections.ExistingUser) = getProjection activationKey |> Result.ofOption (AuthError.ActivationKeyDoesNotMatch |> ServerError.AuthError)
        let! _ = ({ Id = user.Id; ActivationKey = activationKey } : CmdArgs.Activate) |> cmdHandler user
        return user.Id
    }

let initiatePasswordReset getProjection cmdHandler (email:string) =
    result {
        let! (user : Yobo.Core.Auth.Projections.ExistingUser) = getProjection email |> Result.ofOption (AuthError.InvalidLogin |> ServerError.AuthError)
        let! _ = ({ Id = user.Id; PasswordResetKey = Guid.NewGuid() } : CmdArgs.InitiatePasswordReset) |> cmdHandler user
        return ()
    }

let resetPassword getProjection cmdHandler createHashFn (key:Guid,pwdReset:PasswordReset) =
    result {
        let! (user : Yobo.Core.Auth.Projections.ExistingUser) = getProjection key |> Result.ofOption (AuthError.PasswordResetKeyDoesNotMatch |> ServerError.AuthError)
        let! args = pwdReset |> ArgsBuilder.buildPasswordReset user.Id key createHashFn
        let! _ = args |> cmdHandler user
        return ()
    }