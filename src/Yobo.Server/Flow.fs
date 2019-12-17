[<RequireQualifiedAccess>]
module Yobo.Server.Flow

open System.Threading.Tasks
open Yobo.Shared.Domain
open FSharp.Control.Tasks.V2

let toError (ex:exn) = ex.Message |> ServerError.Exception |> Error

let ofTask (cf:'a -> Task<'b>) (v:'a) : Task<ServerResult<'b>> =
    task {
        try
            let! res = v |> cf
            return res |> Ok
        with ex -> return ex |> toError
    }

let ofResult (cf:'a -> ServerResult<'b>) (v:'a) : Task<ServerResult<'b>> =
    task {
        try
            return v |> cf
        with ex -> return ex |> toError
    }

let ofTaskResult (cf:'a -> Task<ServerResult<'b>>) (v:'a) : Task<ServerResult<'b>> =
    task {
        try
            let! res = v |> cf
            return res
        with ex -> return ex |> toError
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