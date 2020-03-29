module Yobo.Server.Core.Database

open System
open System.Data
open FSharp.Control.Tasks
open Dapper.FSharp
open Dapper.FSharp.MSSQL
open Yobo.Server.Core.Domain
open Yobo.Libraries.Tasks
open Yobo.Shared.Errors

module Tables =
    type Users = Yobo.Server.Auth.Database.Tables.Users
    
    [<RequireQualifiedAccess>]
    module Users =
        let name = Yobo.Server.Auth.Database.Tables.Users.name

module Updates =
    let private getUserById (conn:IDbConnection) (i:Guid) =
        select {
            table Tables.Users.name
            where (eq "Id" i)
        }
        |> conn.SelectAsync<Tables.Users>
        |> Task.map Seq.tryHead
        |> Task.map (ServerError.ofOption (DatabaseItemNotFound i))
    
    let creditsAdded (conn:IDbConnection) (args:CmdArgs.AddCredits) =
        task {
            let! user = args.UserId |> getUserById conn
            let userCredits = Yobo.Shared.Core.Domain.calculateCredits user.Credits user.CreditsExpiration
            let newCredits = userCredits + args.Credits
            let! _ =
                update {
                    table Tables.Users.name
                    set {| Credits = newCredits; CreditsExpiration = args.Expiration |}
                    where (eq "Id" args.UserId)
                } |> conn.UpdateAsync
            return ()
        }
        
    let expirationSet (conn:IDbConnection) (args:CmdArgs.SetExpiration) =
        update {
            table Tables.Users.name
            set {| CreditsExpiration = args.Expiration |}
            where (eq "Id" args.UserId)
        }
        |> conn.UpdateAsync
        |> Task.ignore
    

module Projections =
    let getById (conn:IDbConnection) (i:Guid) =
        select {
            table "Users"
            where (eq "Id" i)
        }
        |> conn.SelectAsync<Tables.Users>
        |> Task.map (Seq.toList >> List.map (fun x ->
            {
                Id = x.Id
                IsActivated = x.Activated.IsSome
            } : CommandHandler.Projections.ExistingUser    
        ))
        |> Task.map (List.tryHead >> ServerError.ofOption (DatabaseItemNotFound i))
