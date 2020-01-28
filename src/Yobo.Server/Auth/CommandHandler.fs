module Yobo.Server.Auth.CommandHandler

open System
open Domain
open Yobo.Shared.Domain

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
    |> Option.map (fun usr -> if not usr.IsActivated then Ok [ Activated args ] else Error AuthenticationError.AccountAlreadyActivatedOrNotFound)
    |> Option.defaultValue (Error AuthenticationError.AccountAlreadyActivatedOrNotFound)