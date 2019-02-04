module Yobo.Core.Users.Registry.CommandValidator

open Yobo.Shared.Validation
open FSharp.Rop

let validate cmd =
    match cmd with
    | Add args ->
        [
            "Email", validateEmail (fun (x:CmdArgs.Add) -> x.Email)
            "UserId", validateNotEmptyGuid (fun x -> x.UserId)
        ]
        |> validate args
        <!> (fun _ -> cmd)
    | Remove args ->
        [
            "Email", validateEmail (fun (x:CmdArgs.Remove) -> x.Email)
            "UserId", validateNotEmptyGuid (fun x -> x.UserId)
        ]
        |> validate args
        <!> (fun _ -> cmd)
        