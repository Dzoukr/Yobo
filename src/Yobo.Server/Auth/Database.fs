module Yobo.Server.Auth.Database

open System
open System.Data
open FSharp.Control.Tasks.V2
open Domain
open Dapper.FSharp
open Dapper.FSharp.MSSQL

module Tables =
    type Users = {
        Id : Guid
        Email : string
        FirstName : string
        LastName : string
        PasswordHash : string
        ActivationKey : Guid
        Registered : DateTimeOffset
        Activated : DateTimeOffset option
        Credits : int
        CreditsExpiration : DateTimeOffset option
        CashReservationBlockedUntil : DateTimeOffset option
        PasswordResetKey : Guid option
        Newsletters : bool
    }
    
module Updates =
    let registered (conn:IDbConnection) (args:CmdArgs.Register) =
        task {
            let! _ =
                insert {
                    table "Users"
                    value ({
                        Id = args.Id
                        Email = args.Email
                        FirstName = args.FirstName
                        LastName = args.FirstName
                        PasswordHash = args.PasswordHash
                        ActivationKey = args.ActivationKey
                        Registered = DateTimeOffset.UtcNow
                        Activated = None
                        Credits = 0
                        CreditsExpiration = None
                        CashReservationBlockedUntil = None
                        PasswordResetKey = None
                        Newsletters = args.Newsletters
                    } : Tables.Users)
                } |> conn.InsertAsync
            return ()
        }
    
    let activated (conn:IDbConnection) (args:CmdArgs.Activate) =
        task {
            let! _ =
                update {
                    table "Users"
                    set {| Activated = DateTimeOffset.UtcNow |}
                    where (eq "ActivationKey" args.ActivationKey)
                } |> conn.UpdateAsync
            return ()
        }

module Queries =
    let tryGetUserByEmail (conn:IDbConnection) email =
        task {
            let! res =
                select {
                    table "Users"
                    where (eq "Email" email)
                }
                |> conn.SelectAsync<Tables.Users>
            return
                res
                |> Seq.tryHead
                |> Option.map (fun x ->
                    {
                        Id = x.Id
                        Email = x.Email
                        PasswordHash = x.PasswordHash
                        FirstName = x.FirstName
                        LastName = x.LastName
                    } : Queries.AuthUserView
                )
        }

module Projections =
    let getAll (conn:IDbConnection) =
        task {
            let! res = select { table "Users" } |> conn.SelectAsync<Tables.Users>
            return
                res
                |> Seq.toList
                |> List.map (fun x ->
                    {
                        Id = x.Id
                        Email = x.Email
                        IsActivated = x.Activated.IsSome
                        ActivationKey = x.ActivationKey
                        PasswordResetKey = x.PasswordResetKey
                        Newsletters = x.Newsletters
                    } : CommandHandler.Projections.ExistingUser    
                )
        }