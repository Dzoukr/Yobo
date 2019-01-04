module Yobo.API.Users.Functions

open System
open Yobo.Shared.Users.Domain
open FSharp.Rop
open Yobo.Core.Users
open Yobo.Core
open Yobo.Shared.Auth
open System.Security.Claims
open Yobo.Shared.Communication

let private mapToUser (u:Yobo.Core.Users.ReadQueries.User) =
    {
        Id = u.Id
        Email = u.Email
        FirstName = u.FirstName
        LastName = u.LastName
    }

let private claimsToUser (claims:seq<Claim>) =
    let find key = claims |> Seq.find (fun x -> x.Type = key) |> (fun x -> x.Value)
    {
        Id = find "Id" |> Guid
        Email = find "Email"
        FirstName = find "FirstName"
        LastName = find "LastName"
    }

let private userToClaims (u:User) =
    seq [
        Claim("Id", u.Id.ToString())
        Claim("Email", u.Email)
        Claim("FirstName", u.FirstName)
        Claim("LastName", u.LastName)
    ]

let login loginFn tokenCreator (acc:Login) =
    result {
        let! user = loginFn acc.Email acc.Password
        return user |> mapToUser |> userToClaims |> tokenCreator
    }

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