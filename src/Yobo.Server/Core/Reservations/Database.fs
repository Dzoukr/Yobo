module Yobo.Server.Core.Reservations.Database

open System
open System.Data
open FSharp.Control.Tasks.V2
open Dapper.FSharp
open Dapper.FSharp.MSSQL
open Yobo.Shared.Core.Domain
open Yobo.Shared.Core.Reservations.Domain.Queries

module Queries =
    open Yobo.Server.Auth.Database
    open Yobo.Shared.Core.Reservations.Domain
    open Yobo.Server.Core.Database
    
    let getLessons (conn:IDbConnection) userId (dFrom:DateTimeOffset,dTo:DateTimeOffset) =
        task {
            let! res =
                select {
                    table Tables.Lessons.name
                    leftJoin Tables.LessonReservations.name "LessonId" "Lessons.Id"
                    where (ge "Lessons.StartDate" dFrom + le "Lessons.StartDate" dTo)
                }
                |> conn.SelectAsyncOption<Tables.Lessons, Tables.LessonReservations>
            return
                res
                |> List.ofSeq
                |> List.groupBy fst
                |> List.map (fun (lsn,gr) ->
                    let res = gr |> List.choose snd
                    let userRes = res |> List.tryFind (fun x -> x.UserId = userId)
                                            
                    {
                        Id = lsn.Id
                        StartDate = lsn.StartDate
                        EndDate = lsn.EndDate
                        Name = lsn.Name
                        Description = lsn.Description
                        Availability = Availability.getAvailability lsn.StartDate lsn.IsCancelled lsn.Capacity res.Length
                        UserReservation = userRes |> Option.map (fun x -> Queries.LessonPayment.fromUseCredits x.UseCredits)
                        IsCancelled = lsn.IsCancelled
                        CancellableUntil = lsn.StartDate |> Yobo.Shared.Core.Domain.getCancellingDate
                    } : Queries.Lesson
                )
        }
        
//    let getWorkshops (conn:IDbConnection) (dFrom:DateTimeOffset,dTo:DateTimeOffset) =
//        task {
//            let! res =
//                select {
//                    table Tables.Workshops.name
//                    where (ge "Workshops.StartDate" dFrom + le "Workshops.StartDate" dTo)
//                }
//                |> conn.SelectAsync<Tables.Workshops>
//            return
//                res
//                |> List.ofSeq
//                |> List.map (fun x ->
//                    {
//                        Id = x.Id
//                        StartDate = x.StartDate
//                        EndDate = x.EndDate
//                        Name = x.Name
//                        Description = x.Description
//                    } : Queries.Workshop
//                )
//        }         
    
    let getOnlineLessons (conn:IDbConnection) userId (dFrom:DateTimeOffset,dTo:DateTimeOffset) =
        task {
            let! res =
                select {
                    table Tables.OnlineLessons.name
                    leftJoin Tables.OnlineLessonReservations.name "OnlineLessonId" "OnlineLessons.Id"
                    where (ge "OnlineLessons.StartDate" dFrom + le "OnlineLessons.StartDate" dTo)
                }
                |> conn.SelectAsyncOption<Tables.OnlineLessons, Tables.OnlineLessonReservations>
            return
                res
                |> List.ofSeq
                |> List.groupBy fst
                |> List.map (fun (lsn,gr) ->
                    let res = gr |> List.choose snd
                    let userRes = res |> List.tryFind (fun x -> x.UserId = userId)
                                            
                    {
                        Id = lsn.Id
                        StartDate = lsn.StartDate
                        EndDate = lsn.EndDate
                        Name = lsn.Name
                        Description = lsn.Description
                        Availability = Availability.getAvailability lsn.StartDate lsn.IsCancelled lsn.Capacity res.Length
                        UserReservation = userRes |> Option.map (fun x -> Queries.LessonPayment.fromUseCredits x.UseCredits)
                        IsCancelled = lsn.IsCancelled
                        CancellableUntil = lsn.StartDate |> Yobo.Shared.Core.Domain.getCancellingDate
                    } : Queries.OnlineLesson
                )
        } 
