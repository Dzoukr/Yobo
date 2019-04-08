module Yobo.Core.Workshops.Aggregate

open FSharp.Rop
open Yobo.Shared.Domain
open Yobo.Core.Workshops
open System

let private onlyIfDoesNotExist state =
    if state.Id = State.Init.Id then Ok state
    else DomainError.ItemAlreadyExists "Id" |> Error

let private onlyIfDoesExist state =
    if state.Id <> State.Init.Id then Ok state
    else DomainError.ItemDoesNotExist "Id" |> Error

let private onlyIfNotAlreadyDeleted state =
    if state.IsDeleted then DomainError.WorkshopIsAlreadyDeleted |> Error
    else Ok state

let execute (state:State) = function
    | Create args ->
        onlyIfDoesNotExist state
        <!> (fun _ -> Created args)
        <!> List.singleton
    | Delete args ->
        onlyIfDoesExist state
        >>= onlyIfNotAlreadyDeleted
        <!> (fun _ -> Deleted args)
        <!> List.singleton
    
let apply (state:State) = function
    | Created args -> { state with Id = args.Id; StartDate = args.StartDate; EndDate = args.EndDate }
    | Deleted _ -> { state with IsDeleted = true }
