module Yobo.API.Auth.Functions

open System
open Yobo.Shared.Auth.Domain
open FSharp.Rop
open Yobo.Core.Users
open Yobo.Core
open Yobo.Shared.Auth
open System.Security.Claims
open Yobo.Shared.Communication

let mapToUser (u:Yobo.Core.Users.ReadQueries.User) =
    {
        Id = u.Id
        Email = u.Email
        FirstName = u.FirstName
        LastName = u.LastName
        IsAdmin = false
    } : Yobo.Shared.Domain.User

let claimsToUser (claims:seq<Claim>) =
    let find key = claims |> Seq.find (fun x -> x.Type = key) |> (fun x -> x.Value)
    {
        Id = find "Id" |> Guid
        Email = find "Email"
        FirstName = find "FirstName"
        LastName = find "LastName"
        IsAdmin = find "IsAdmin" |> Boolean.Parse
    } : Yobo.Shared.Domain.User

let userToClaims (u:Yobo.Shared.Domain.User) =
    seq [
        Claim("Id", u.Id.ToString())
        Claim("Email", u.Email)
        Claim("FirstName", u.FirstName)
        Claim("LastName", u.LastName)
        Claim("IsAdmin", (u.IsAdmin.ToString()))
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

let getUser validateFn token =
    match token |> validateFn with
    | Some claims -> claims |> claimsToUser |> Ok
    | None -> AuthError.InvalidOrExpiredToken |> ServerError.AuthError |> Error

let register cmdHandler createHashFn (acc:NewAccount) =
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