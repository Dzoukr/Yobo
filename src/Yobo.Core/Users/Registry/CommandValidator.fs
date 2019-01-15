module Yobo.Core.Users.Registry.CommandValidator

open Yobo.Shared.Validation
open FSharp.Rop

let validate cmd =
    match cmd with
    | Add args ->
        [ validateEmail "Email" (fun (x:CmdArgs.Add) -> x.Email); validateNotEmptyGuid "UserId" (fun x -> x.UserId) ]
        |> validate args
        <!> (fun _ -> cmd)
    | Remove args ->
        [ validateEmail "Email" (fun (x:CmdArgs.Remove) -> x.Email); validateNotEmptyGuid "UserId" (fun x -> x.UserId) ]
        |> validate args
        <!> (fun _ -> cmd)
        