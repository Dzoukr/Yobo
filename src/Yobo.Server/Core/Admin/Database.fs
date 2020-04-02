module Yobo.Server.Core.Admin.Database

open System
open System.Data
open FSharp.Control.Tasks.V2
open Dapper.FSharp
open Dapper.FSharp.MSSQL
open Yobo.Libraries.Tuples

module Queries =
    open Yobo.Server.Auth.Database
    open Yobo.Shared.Core.Admin.Domain
    open Yobo.Server.Core.Database
    
    let private toUser (x:Tables.Users) : Queries.User =
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
    
    let private toLessonPayment (x:Tables.LessonReservations) : Queries.LessonPayment =
        if x.UseCredits then Queries.LessonPayment.Credits
        else Queries.LessonPayment.Cash
    
    let private toLessonPaymentFromOnline (x:Tables.OnlineLessonReservations) : Queries.LessonPayment =
        if x.UseCredits then Queries.LessonPayment.Credits
        else Queries.LessonPayment.Cash
    
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
                |> List.map toUser
        }
        
    let getLessons (conn:IDbConnection) (dFrom:DateTimeOffset,dTo:DateTimeOffset) =
        task {
            let! res =
                select {
                    table Tables.Lessons.name
                    leftJoin Tables.LessonReservations.name "LessonId" "Lessons.Id"
                    leftJoin Tables.Users.name "Id" "LessonReservations.UserId"
                    where (ge "Lessons.StartDate" dFrom + le "Lessons.StartDate" dTo)
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
                        |> List.map (fun (res,usr) ->
                            (usr |> toUser), (res |> toLessonPayment)
                        )
                    {
                        Id = lsn.Id
                        StartDate = lsn.StartDate
                        EndDate = lsn.EndDate
                        Name = lsn.Name
                        Description = lsn.Description
                        Reservations = res
                        IsCancelled = lsn.IsCancelled
                        Capacity = lsn.Capacity
                    } : Queries.Lesson
                )
        }
        
    let getWorkshops (conn:IDbConnection) (dFrom:DateTimeOffset,dTo:DateTimeOffset) =
        task {
            let! res =
                select {
                    table Tables.Workshops.name
                    where (ge "Workshops.StartDate" dFrom + le "Workshops.StartDate" dTo)
                }
                |> conn.SelectAsync<Tables.Workshops>
            return
                res
                |> List.ofSeq
                |> List.map (fun x ->
                    {
                        Id = x.Id
                        StartDate = x.StartDate
                        EndDate = x.EndDate
                        Name = x.Name
                        Description = x.Description
                    } : Queries.Workshop
                )
        }         
    
    let getOnlineLessons (conn:IDbConnection) (dFrom:DateTimeOffset,dTo:DateTimeOffset) =
        task {
            let! res =
                select {
                    table Tables.OnlineLessons.name
                    leftJoin Tables.OnlineLessonReservations.name "OnlineLessonId" "OnlineLessons.Id"
                    leftJoin Tables.Users.name "Id" "OnlineLessonReservations.UserId"
                    where (ge "OnlineLessons.StartDate" dFrom + le "OnlineLessons.StartDate" dTo)
                }
                |> conn.SelectAsyncOption<Tables.OnlineLessons, Tables.OnlineLessonReservations, Tables.Users>
            return
                res
                |> List.ofSeq
                |> List.groupBy fstOf3
                |> List.map (fun (lsn,gr) ->
                    let res =
                        gr
                        |> List.choose (ignoreFstOf3 >> optionOf2)
                        |> List.map (fun (res,usr) ->
                            (usr |> toUser), (res |> toLessonPaymentFromOnline)
                        )
                    {
                        Id = lsn.Id
                        StartDate = lsn.StartDate
                        EndDate = lsn.EndDate
                        Name = lsn.Name
                        Description = lsn.Description
                        Reservations = res
                        IsCancelled = lsn.IsCancelled
                        Capacity = lsn.Capacity
                    } : Queries.OnlineLesson
                )
        } 
