module Yobo.Server.Auth.CommandHandler

open System
open Domain
open Yobo.Shared.Errors
open FSharp.Rop.Result
open FSharp.Rop.Result.Operators

module Projections =
    type ExistingUser = {
        Id : Guid
        Email : string
        IsActivated : bool
        ActivationKey : Guid
        PasswordResetKey : Guid option
        Newsletters : bool
    }

let private tryFindById (allUsers:Projections.ExistingUser list) (id:Guid) =
    allUsers |> List.tryFind (fun x -> x.Id = id)

let private tryFindByEmail (allUsers:Projections.ExistingUser list) (email:string) =
    allUsers |> List.tryFind (fun x -> x.Email.ToUpperInvariant() = email.ToUpperInvariant())

let private tryFindByActivationKey (allUsers:Projections.ExistingUser list) (key:Guid) =
    allUsers |> List.tryFind (fun x -> x.ActivationKey = key)

let private tryFindByResetKey (allUsers:Projections.ExistingUser list) (key:Guid) =
    allUsers |> List.tryFind (fun x -> x.PasswordResetKey = Some key)

let private isUniqueKey (allUsers:Projections.ExistingUser list) (key:Guid) =
    key |> tryFindByResetKey allUsers |> Option.isNone

let private onlyIfNotAlreadyActivated (user:Projections.ExistingUser) =
    if user.IsActivated then AuthenticationError.AccountAlreadyActivatedOrNotFound |> Error else Ok user

let register (allUsers:Projections.ExistingUser list) (args:CmdArgs.Register) =
    [
        tryFindById allUsers args.Id
        tryFindByEmail allUsers args.Email
        tryFindByActivationKey allUsers args.ActivationKey
    ]
    |> List.choose id
    |> function
        | [] -> [ Registered args ] |> Ok
        | _ -> AuthenticationError.EmailAlreadyRegistered |> Error
        
let activate (allUsers:Projections.ExistingUser list) (args:CmdArgs.Activate) =
    args.ActivationKey
    |> tryFindByActivationKey allUsers
    |> Option.bind (fun usr ->
        if not usr.IsActivated then
            Some [
                Activated { Id = usr.Id }
                if usr.Newsletters then SubscribedToNewsletters { Id = usr.Id }
            ]
            else None
    )
    |> Result.ofOption AuthenticationError.AccountAlreadyActivatedOrNotFound
    
let initiatePasswordReset (allUsers:Projections.ExistingUser list) (args:CmdArgs.InitiatePasswordReset) =
    args.Email
    |> tryFindByEmail allUsers
    |> Option.map (fun usr ->
        if isUniqueKey allUsers args.PasswordResetKey then
            [ PasswordResetInitiated { Id = usr.Id; PasswordResetKey = args.PasswordResetKey } ]
        else []
    )
    |> Option.defaultValue [] // Note: security - we cannot say we didn't found account!
    |> Ok
    
let completePasswordReset (allUsers:Projections.ExistingUser list) (args:CmdArgs.CompleteResetPassword) =
    args.PasswordResetKey
    |> tryFindByResetKey allUsers
    |> Option.map (fun usr -> [ PasswordResetComplete { Id = usr.Id; PasswordResetKey = args.PasswordResetKey; PasswordHash = args.PasswordHash } ] )
    |> Result.ofOption AuthenticationError.InvalidPasswordResetKey

let regenerateActivationKey (allUsers:Projections.ExistingUser list) (args:CmdArgs.RegenerateActivationKey) =
    args.Id
    |> tryFindById allUsers
    |> Result.ofOption AuthenticationError.AccountAlreadyActivatedOrNotFound
    >>= onlyIfNotAlreadyActivated
    |>> (fun _ -> [ ActivationKeyRegenerated args ])