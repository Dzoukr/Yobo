module Yobo.Server.Auth.Database

open System.Data
open FSharp.Control.Tasks.V2
open Domain
open Yobo.Libraries.SimpleDapper

module Queries =
    let tryGetUserByEmail (conn:IDbConnection) email =
        task {
            let! res =
                select<Queries.AuthUserView> "Users"
                |> where (Column ("Email", Eq(email)))
                |> conn.SelectAsync
            return res |> Seq.tryHead
        }

module Projections =
    let getAll (conn:IDbConnection) =
        task {
            let! res = select<CommandHandler.Projections.ExistingUser> "Users" |> conn.SelectAsync
            return res |> Seq.toList
        }