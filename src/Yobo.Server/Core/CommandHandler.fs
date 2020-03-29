module Yobo.Server.Core.CommandHandler

open System
open Domain
open Yobo.Shared.Errors
open FSharp.Rop.Result.Operators

module Projections =
    type ExistingUser = {
        Id : Guid
        IsActivated : bool
    }

let private onlyIfActivated (user:Projections.ExistingUser) =
    if user.IsActivated then Ok user else ServerError.UserNotActivated |> Error
    
let addCredits (user:Projections.ExistingUser) (args:CmdArgs.AddCredits) =
    user
    |> onlyIfActivated
    |>> (fun _ -> CreditsAdded args)
    |>> List.singleton    