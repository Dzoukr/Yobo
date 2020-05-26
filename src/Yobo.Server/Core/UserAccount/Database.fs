module Yobo.Server.Core.UserAccount.Database

open System
open System.Data
open FSharp.Control.Tasks.V2
open Dapper.FSharp
open Dapper.FSharp.MSSQL
open Yobo.Server.Core.Database
open Yobo.Shared.Core.Domain

module Queries =
    open Yobo.Server.Auth.Database
    open Yobo.Shared.Core.UserAccount.Domain
    
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
                        Email = x.Email
                        FirstName = x.FirstName
                        LastName = x.LastName
                        IsActivated = x.Activated.IsSome
                        Credits = x.Credits
                        CreditsExpiration = x.CreditsExpiration
                        IsAdmin = false
                    } : Queries.UserAccount
                )
        }
        
    let getLessonsForUserId (conn:IDbConnection) (userId:Guid) =
        task {
            let! res =
                select {
                    table Tables.Lessons.name
                    innerJoin Tables.LessonReservations.name "LessonId" "Lessons.Id"
                    where (gt "Lessons.EndDate" DateTimeOffset.Now + eq "LessonReservations.UserId" userId)
                }
                |> conn.SelectAsync<Tables.Lessons, Tables.LessonReservations>
            return
                res
                |> List.ofSeq
                |> List.map (fun (lsn,gr) ->
                    {
                        Id = lsn.Id
                        StartDate = lsn.StartDate
                        EndDate = lsn.EndDate
                        Name = lsn.Name
                        Description = lsn.Description
                        Payment = Queries.LessonPayment.fromUseCredits gr.UseCredits
                    } : Queries.Lesson
                )
        }