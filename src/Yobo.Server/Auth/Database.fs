module Yobo.Server.Auth.Database

open System
open System.Data
open Dapper
open FSharp.Control.Tasks.V2
open Domain

module Queries =
    let tryGetUserByEmail (conn:IDbConnection) email =
        task {
            let query = "SELECT * FROM [Users] WHERE Email = @Email"
            let! res = conn.QueryAsync<Queries.AuthUserView>(query, {| Email = email |})
            return res |> Seq.tryHead
        }

module Projections =
    let getAll (conn:IDbConnection) =
        task {
            let query = "SELECT * FROM [Users]"
            let! res = conn.QueryAsync<CommandHandler.Projections.ExistingUser>(query)
            return res |> Seq.toList
        }