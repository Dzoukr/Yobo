module Yobo.Core.Auth.CommandHandler

open System
open Projections
open Yobo.Shared.Domain
open FSharp.Rop

let private tryFindById (allUsers:ExistingUser list) (id:Guid) =
    allUsers
    |> List.tryFind (fun x -> x.Id = id)

let private tryFindByEmail (allUsers:ExistingUser list) (email:string) =
    allUsers
    |> List.tryFind (fun x -> x.Email.ToUpperInvariant() = email.ToUpperInvariant())

let private onlyIfNotAlreadyActivated user =
    if user.IsActivated then DomainError.UserAlreadyActivated |> Error else Ok user

let private onlyIfActivated user =
    if user.IsActivated then Ok user else DomainError.UserNotActivated |> Error

let onlyIfPasswordResetKeyMatch key user =
    if user.PasswordResetKey = Some key then Ok user else DomainError.PasswordResetKeyDoesNotMatch |> Error

let private onlyIfActivationKeyMatch key user =
    if user.ActivationKey = key then Ok user else DomainError.ActivationKeyDoesNotMatch |> Error

let register (allUsers:ExistingUser list) (args:CmdArgs.Register) =
    match tryFindById allUsers args.Id, tryFindByEmail allUsers args.Email with
    | Some e, Some _
    | Some e, None
    | None, Some e -> DomainError.ItemAlreadyExists "Email" |> Error
    | None, None -> [ Registered args ] |> Ok

let regenerateActivationKey (user:ExistingUser) (args:CmdArgs.RegenerateActivationKey) =
    user
    |> onlyIfNotAlreadyActivated
    <!> (fun _ -> ActivationKeyRegenerated args)
    <!> List.singleton

let activate (user:ExistingUser) (args:CmdArgs.Activate) =
    user
    |> onlyIfNotAlreadyActivated
    >>= onlyIfActivationKeyMatch args.ActivationKey
    <!> (fun _ -> Activated args)
    <!> List.singleton

let initiatePasswordReset (user:ExistingUser) (args:CmdArgs.InitiatePasswordReset) =
    user
    |> onlyIfActivated
    <!> (fun _ -> PasswordResetInitiated args)
    <!> List.singleton

let resetPassword (user:ExistingUser) (args:CmdArgs.ResetPassword) =
    user
    |> onlyIfActivated
    >>= onlyIfPasswordResetKeyMatch args.PasswordResetKey
    <!> (fun _ -> PasswordReset args)
    <!> List.singleton