module Yobo.Core.Lessons.Aggregate

open FSharp.Rop
open Yobo.Shared.Domain
open Yobo.Core.Lessons

let onlyIfDoesNotExist state =
    if state.Id = State.Init.Id then Ok state
    else DomainError.ItemAlreadyExists "Id" |> Error

let execute (state:State) = function
    | Create args ->
        onlyIfDoesNotExist state
        <!> (fun _ -> Created args)
        <!> List.singleton
    
let apply (state:State) = function
    | Created args -> { state with Id = args.Id }