[<RequireQualifiedAccess>]
module Yobo.Server.Flow

open System.Threading.Tasks
open Yobo.Shared.Communication
open FSharp.Control.Tasks.V2

let ofTask (cf:'a -> Task<'b>) (v:'a) : Task<ServerResult<'b>> =
    task {
        try
            let! res = v |> cf
            return res |> Ok
        with ex ->
            return ex.Message |> ServerError.Exception |> Error
    }

let ofTaskResult (cf:'a -> Task<ServerResult<'b>>) (v:'a) : Task<ServerResult<'b>> =
    task {
        try
            let! res = v |> cf
            return res
        with ex ->
            return ex.Message |> ServerError.Exception |> Error
    }

let ofTaskValidated vf cf p =
    task {
        match p |> ServerResult.ofValidation vf with
        | Ok v -> return! ofTask cf v
        | Error e -> return Error e
    }

let ofTaskResultValidated vf cf p =
    task {
        match p |> ServerResult.ofValidation vf with
        | Ok v -> return! ofTaskResult cf v
        | Error e -> return Error e
    }