module Yobo.Core.Users.Registry.CommandValidator

open Yobo.Shared.Text
open Yobo.Shared.Validation
open FSharp.Rop

let validate cmd =
    match cmd with
    | Add args ->
        [ validateEmail TextValue.Email (fun (x:CmdArgs.Add) -> x.Email); validateNotEmptyGuid TextValue.Id (fun x -> x.UserId) ]
        |> validate args
        <!> (fun _ -> cmd)
    | Remove args ->
        [ validateEmail TextValue.Email (fun (x:CmdArgs.Remove) -> x.Email); validateNotEmptyGuid TextValue.Id (fun x -> x.UserId) ]
        |> validate args
        <!> (fun _ -> cmd)
        