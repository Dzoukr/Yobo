module Yobo.Core.Users.Registry.Aggregate

open FSharp.Rop
open Yobo.Shared.Domain
open Yobo.Core.Users.Registry

let onlyIfDoesNotExist (state:State) (email:string) =
    if state.Emails |> List.exists (fun (_,em) -> email = em) then DomainError.ItemAlreadyExists "Email" |> Error
    else Ok email

let onlyIfExists (state:State) userId (email:string) =
    if state.Emails |> List.exists (fun (i,em) -> userId = i && email = em) then Ok email
    else DomainError.ItemDoesNotExist "Email" |> Error

let normalizeAdd (args:CmdArgs.Add) = { args with Email = args.Email.ToLower() }
let normalizeRemove (args:CmdArgs.Remove) = { args with Email = args.Email.ToLower() }

let execute (state:State) = function
    | Add args ->
        let args = normalizeAdd args
        onlyIfDoesNotExist state args.Email <!> (fun _ -> Added args) <!> List.singleton
    | Remove args ->
        let args = normalizeRemove args
        onlyIfExists state args.UserId args.Email <!> (fun _ -> Removed args) <!> List.singleton

let apply (state:State) = function
    | Added args -> { state with Emails = (args.UserId, args.Email) :: state.Emails }
    | Removed args -> { state with Emails = state.Emails |> List.filter (fun (_,e) -> e <> args.Email) }
