module Yobo.Server.UserAccount.Database

open System
open System.Data
open FSharp.Control.Tasks.V2
open Domain
open Dapper.FSharp
open Dapper.FSharp.MSSQL

module Queries =
    open Yobo.Server.Auth.Database
    open Yobo.Shared.UserAccount.Domain
    
    let tryGetUserById (conn:IDbConnection) (id:Guid) =
        task {
            let! res =
                select {
                    table Tables.Users.name
                    where (eq "Id" id)
                }
                |> conn.SelectAsync<Tables.Users>
            return
                res
                |> Seq.tryHead
                |> Option.map (fun x ->
                    {
                        Id = x.Id
                        FirstName = x.FirstName
                        LastName = x.LastName
                        IsActivated = x.Activated.IsSome
                        Credits = x.Credits
                        CreditsExpiration = x.CreditsExpiration
                    } : Queries.UserAccount
                )
        }        
        
//
//module Projections =
//    let getAll (conn:IDbConnection) =
//        task {
//            let! res = select { table "Users" } |> conn.SelectAsync<Tables.Users>
//            return
//                res
//                |> Seq.toList
//                |> List.map (fun x ->
//                    {
//                        Id = x.Id
//                        Email = x.Email
//                        IsActivated = x.Activated.IsSome
//                        ActivationKey = x.ActivationKey
//                        PasswordResetKey = x.PasswordResetKey
//                        Newsletters = x.Newsletters
//                    } : CommandHandler.Projections.ExistingUser    
//                )
//        }