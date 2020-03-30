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
    
    let private toReservation (x:Tables.LessonReservations) : Queries.LessonReservation =
        if x.UseCredits then Queries.LessonReservation.Credits
        else Queries.LessonReservation.Cash
    
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
                    where (ge "Lessons.StartDate" dFrom + le "Lessons.EndDate" dTo)
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
                            (usr |> toUser), (res |> toReservation)
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
