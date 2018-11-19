module Yobo.Core.Users.Aggregate

open FSharp.Rop
open Yobo.Shared.Domain
open Yobo.Core.Users

let onlyIfDoesNotExist state =
    if state.Id = State.Init.Id then Ok state
    else DomainError.ItemAlreadyExists |> Error

let normalizeCreate (args:CmdArgs.Register) = { args with Email = args.Email.ToLower() }

let execute (state:State) = function
    | Register args ->
        onlyIfDoesNotExist state
        <!> (fun _ -> normalizeCreate args)
        <!> (fun a -> Registered a)
        <!> List.singleton

let apply (state:State) = function
    | Registered args -> { state with Id = args.Id }
