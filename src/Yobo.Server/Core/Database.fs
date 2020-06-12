module Yobo.Server.Core.Database

open System
open System.Data
open FSharp.Control.Tasks
open Dapper.FSharp
open Dapper.FSharp.MSSQL
open Yobo.Libraries
open Yobo.Server.Core.Domain
open Yobo.Libraries.Tasks
open Yobo.Shared.Tuples
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
        CancellableBeforeStart : TimeSpan
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
        
    type Workshops = {
        Id : Guid
        Name : string
        Description : string
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Created : DateTimeOffset
    }         
    
    [<RequireQualifiedAccess>]
    module Workshops =
        let name = "Workshops"

let internal getUserById (conn:IDbConnection) (i:Guid) =
    select {
        table Tables.Users.name
        where (eq "Id" i)
    }
    |> conn.SelectAsync<Tables.Users>
    |> Task.map Seq.tryHead
    |> Task.map (ServerError.ofOption (DatabaseItemNotFound i))
    |> Task.map (fun x -> { x with Credits = Yobo.Shared.Core.Domain.calculateCredits x.Credits x.CreditsExpiration })
    
module Updates =
    let creditsAdded (conn:IDbConnection) (args:CmdArgs.AddCredits) =
        task {
            let! user = args.UserId |> getUserById conn
            let newCredits = user.Credits + args.Credits
            let! _ =
                update {
                    table Tables.Users.name
                    set {| Credits = newCredits; CreditsExpiration = args.Expiration |}
                    where (eq "Id" args.UserId)
                } |> conn.UpdateAsync
            return ()
        }
    
    let creditWithdrawn (conn:IDbConnection) (args:CmdArgs.WithdrawCredit) =
        task {
            let! user = args.UserId |> getUserById conn
            let newCredits = user.Credits - 1
            let! _ =
                update {
                    table Tables.Users.name
                    set {| Credits = newCredits |}
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
                CancellableBeforeStart = args.CancellableBeforeStart
            } : Tables.Lessons
        insert {
            table Tables.Lessons.name
            value newValue
        }
        |> conn.InsertAsync
        |> Task.ignore        
    
    let workshopCreated (conn:IDbConnection) (args:CmdArgs.CreateWorkshop) =
        let newValue =
            {
                Id = args.Id
                Name = args.Name
                Description = args.Description
                StartDate = args.StartDate
                EndDate = args.EndDate
                Created = DateTimeOffset.UtcNow
            } : Tables.Workshops
        insert {
            table Tables.Workshops.name
            value newValue
        }
        |> conn.InsertAsync
        |> Task.ignore        
        
    let lessonDescriptionChanged (conn:IDbConnection) (args:CmdArgs.ChangeLessonDescription) =
        update {
            table Tables.Lessons.name
            set {| Name = args.Name; Description = args.Description |}
            where (eq "Id" args.Id)
        }
        |> conn.UpdateAsync
        |> Task.ignore
        
    let lessonCancelled (conn:IDbConnection) (args:CmdArgs.CancelLesson) =
        update {
            table Tables.Lessons.name
            set {| IsCancelled = true |}
            where (eq "Id" args.Id)
        }
        |> conn.UpdateAsync
        |> Task.ignore
    
    let cashReservationsUnblocked (conn:IDbConnection) (args:CmdArgs.UnblockCashReservations) =
        update {
            table Tables.Users.name
            set {| CashReservationBlockedUntil = None |}
            where (eq "Id" args.UserId)
        }
        |> conn.UpdateAsync
        |> Task.ignore        
    
    let cashReservationsBlocked (conn:IDbConnection) (args:CmdArgs.BlockCashReservations) =
        update {
            table Tables.Users.name
            set {| CashReservationBlockedUntil = args.Expires |}
            where (eq "Id" args.UserId)
        }
        |> conn.UpdateAsync
        |> Task.ignore        

    let creditRefunded (conn:IDbConnection) (args:CmdArgs.RefundCredit) =
        task {
            let! user = args.UserId |> getUserById conn
            let newCredits = user.Credits + 1
            let! _ =
                update {
                    table Tables.Users.name
                    set {| Credits = newCredits |}
                    where (eq "Id" args.UserId)
                } |> conn.UpdateAsync
            return ()
        }
    
    let lessonReservationCancelled (conn:IDbConnection) (args:CmdArgs.CancelLessonReservation) =
        delete {
            table Tables.LessonReservations.name
            where (eq "LessonId" args.LessonId + eq "UserId" args.UserId)
        }
        |> conn.DeleteAsync
        |> Task.ignore
    
    let lessonReservationAdded (conn:IDbConnection) (args:CmdArgs.AddLessonReservation) =
        insert {
            table Tables.LessonReservations.name
            value {| LessonId = args.LessonId; UserId = args.UserId; UseCredits = args.UseCredits; Created = DateTimeOffset.UtcNow |}
        }
        |> conn.InsertAsync
        |> Task.ignore
        
    let lessonDeleted (conn:IDbConnection) (args:CmdArgs.DeleteLesson) =
        delete {
            table Tables.Lessons.name
            where (eq "Id" args.Id)
        }
        |> conn.DeleteAsync
        |> Task.ignore        

    let workshopDeleted (conn:IDbConnection) (args:CmdArgs.DeleteWorkshop) =
        delete {
            table Tables.Workshops.name
            where (eq "Id" args.Id)
        }
        |> conn.DeleteAsync
        |> Task.ignore        

module Projections =
    let getById (conn:IDbConnection) (i:Guid) =
        i
        |> getUserById conn
        |> Task.map (fun x ->
            ({
                Id = x.Id
                IsActivated = x.Activated.IsSome
                Credits = x.Credits
                CreditsExpiration = x.CreditsExpiration
                CashReservationBlockedUntil = x.CashReservationBlockedUntil
            } : CommandHandler.Projections.ExistingUser)    
        )
        
    let getWorkshopById (conn:IDbConnection) (i:Guid) =
        select {
            table Tables.Workshops.name
            where (eq "Id" i)
        }
        |> conn.SelectAsync<Tables.Workshops>
        |> Task.map (Seq.toList >> List.map (fun x ->
            {
                Id = x.Id
            } : CommandHandler.Projections.ExistingWorkshop    
        ))
        |> Task.map (List.tryHead >> ServerError.ofOption (DatabaseItemNotFound i))
    
    let private toUserReservation (res:Tables.LessonReservations,usr:Tables.Users) : CommandHandler.Projections.UserReservation =
        {
            UserId = usr.Id
            CreditsExpiration = usr.CreditsExpiration
            UseCredits = res.UseCredits
        }
    
    let private toLesson (lsn:Tables.Lessons,gr) =
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
            Capacity = lsn.Capacity
            CancellableBeforeStart = lsn.CancellableBeforeStart
        } : CommandHandler.Projections.ExistingLesson
    
    let getLessonById (conn:IDbConnection) (i:Guid) =
        task {
            let! res =
                select {
                    table Tables.Lessons.name
                    leftJoin Tables.LessonReservations.name "LessonId" "Lessons.Id"
                    leftJoin Tables.Users.name "Id" "LessonReservations.UserId"
                    where (eq "Lessons.Id" i)
                }
                |> conn.SelectAsyncOption<Tables.Lessons, Tables.LessonReservations, Tables.Users>
            return
                res
                |> List.ofSeq
                |> List.groupBy fstOf3
                |> List.map toLesson
                |> (List.tryHead >> ServerError.ofOption (DatabaseItemNotFound i))
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
                |> List.map toLesson
        }
        