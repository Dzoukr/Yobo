module Yobo.Server.Core.Database

open System
open System.Data
open FSharp.Control.Tasks
open Dapper.FSharp
open Dapper.FSharp.MSSQL
open Yobo.Libraries
open Yobo.Server.Core.Domain
open Yobo.Libraries.Tasks
open Yobo.Libraries.Tuples
open Yobo.Shared.Errors

module Tables =
    type Users = Yobo.Server.Auth.Database.Tables.Users
    
    [<RequireQualifiedAccess>]
    module Users =
        let name = Yobo.Server.Auth.Database.Tables.Users.name
        
    type Lessons = {
        Id : Guid
        Name : string
        Description : string
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Created : DateTimeOffset
        IsCancelled : bool
        Capacity : int
    }
    
    [<RequireQualifiedAccess>]
    module Lessons =
        let name = "Lessons"
        
    type LessonReservations = {
        LessonId : Guid
        UserId : Guid
        Created : DateTimeOffset
        UseCredits : bool
    }
    
    [<RequireQualifiedAccess>]
    module LessonReservations =
        let name = "LessonReservations"           

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
        
    let lessonCreated (conn:IDbConnection) (args:CmdArgs.CreateLesson) =
        let newValue =
            {
                Id = args.Id
                Name = args.Name
                Description = args.Description
                StartDate = args.StartDate
                EndDate = args.EndDate
                Created = DateTimeOffset.UtcNow
                IsCancelled = false
                Capacity = args.Capacity
            } : Tables.Lessons
        insert {
            table Tables.Lessons.name
            value newValue
        }
        |> conn.InsertAsync
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
    
    let private toUserReservation (res:Tables.LessonReservations,usr:Tables.Users) : CommandHandler.Projections.UserReservation =
        {
            UserId = usr.Id
            CreditsExpiration = usr.CreditsExpiration
            UseCredits = res.UseCredits
        }
            
    let getAllLessons (conn:IDbConnection) () =
        task {
            let! res =
                select {
                    table Tables.Lessons.name
                    leftJoin Tables.LessonReservations.name "LessonId" "Lessons.Id"
                    leftJoin Tables.Users.name "Id" "LessonReservations.UserId"
                }
                |> conn.SelectAsyncOption<Tables.Lessons, Tables.LessonReservations, Tables.Users>
            return
                res
                |> List.ofSeq
                |> List.groupBy fstOf3
                |> List.map (fun (lsn,gr) ->
                    let res =
                        gr
                        |> List.choose (ignoreFstOf3 >> optionOf2)
                        |> List.map toUserReservation
                    {
                        Id = lsn.Id
                        StartDate = lsn.StartDate
                        EndDate = lsn.EndDate
                        IsCancelled = lsn.IsCancelled
                        Reservations = res
                    } : CommandHandler.Projections.ExistingLesson
                )
        }
