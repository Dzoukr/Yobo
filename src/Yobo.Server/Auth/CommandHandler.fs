module Yobo.Server.Auth.CommandHandler

open System
open Events
open Yobo.Shared.Domain

module Projections =
    [<CLIMutable>]
    type ExistingUser = {
        Id : Guid
        Email : string
        IsActivated : bool
        ActivationKey : Guid
        PasswordResetKey : Guid option
        Newsletters : bool
    }

let private tryFindById (allUsers:Projections.ExistingUser list) (id:Guid) =
    allUsers
    |> List.tryFind (fun x -> x.Id = id)

let private tryFindByEmail (allUsers:Projections.ExistingUser list) (email:string) =
    allUsers
    |> List.tryFind (fun x -> x.Email.ToUpperInvariant() = email.ToUpperInvariant())

let register (allUsers:Projections.ExistingUser list) (args:CmdArgs.Register) =
    match tryFindById allUsers args.Id, tryFindByEmail allUsers args.Email with
    | Some _, Some _
    | Some _, None
    | None, Some _ -> AuthenticationError.EmailAlreadyRegistered |> Error
    | None, None -> [ Registered args ] |> Ok