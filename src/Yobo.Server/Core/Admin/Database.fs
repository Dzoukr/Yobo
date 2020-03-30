module Yobo.Server.Core.Admin.Database

open System
open System.Data
open FSharp.Control.Tasks.V2
open Dapper.FSharp
open Dapper.FSharp.MSSQL

module Tables =
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
        
    let getAllLessons (conn:IDbConnection) () =
        task {
            let! res =
                select {
                    table Tables.Lessons.name
                    leftJoin 
                }
                |> conn.SelectAsync<Tables.Lessons>
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
                    } : Queries.Lesson
                )
        }         
