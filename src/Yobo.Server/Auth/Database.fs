module Yobo.Server.Auth.Database

open System
open System.Data
open Dapper
open FSharp.Control.Tasks.V2

module Queries =
    [<CLIMutable>]
    type AuthUserView = {
        Id : Guid
        Email : string
        PasswordHash : string
        FirstName : string
        LastName : string
    }
    
    let tryGetUserByEmail (conn:IDbConnection) email =
        task {
            let query = "SELECT * FROM [Users] WHERE Email = @Email"
            let! res = conn.QueryAsync<AuthUserView>(query, {| Email = email |})
            return res |> Seq.tryHead
        }
