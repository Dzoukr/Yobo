module Yobo.Server.Core.Admin.Database

open System
open System.Data
open FSharp.Control.Tasks.V2
open Dapper.FSharp
open Dapper.FSharp.MSSQL

module Queries =
    open Yobo.Server.Auth.Database
    open Yobo.Shared.Core.Admin.Domain
    
    let getAllUsers (conn:IDbConnection) () =
        task {
            let! res =
                select {
                    table Tables.Users.name
                }
                |> conn.SelectAsync<Tables.Users>
            return
                res
                |> List.ofSeq
                |> List.map (fun x ->
                    {
                        Id = x.Id
                        Email = x.Email
                        FirstName = x.FirstName
                        LastName = x.LastName
                        Activated = x.Activated
                        Credits = x.Credits
                        CreditsExpiration = x.CreditsExpiration
                        CashReservationBlockedUntil = x.CashReservationBlockedUntil
                    } : Queries.User
                )
        }        
