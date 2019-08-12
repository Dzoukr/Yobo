module Yobo.Core.Lessons.ReadQueries

open Yobo.Core
open System
open Yobo.Shared.Domain
open Yobo.Shared.Admin.Domain
open Extensions

module Lessons =

    type LessonsQueries = {
        GetAllForDateRange : (DateTimeOffset * DateTimeOffset) -> Lesson list
    }

    let private reservationFromDbEntity (r:ReadDb.Db.dataContext.``dbo.LessonReservationsEntity``) =
        let res =
            match r.Count, r.UseCredits with
            | 2, _ -> ForTwo
            | 1, true -> ForOne(Credits)
            | 1, false -> ForOne(Cash)
            | _ -> failwith "Invalid value in DB for reservations"
        let user = r.``dbo.Users by Id`` |> Seq.head |> Auth.ReadQueries.userFromDbEntity
        user, res

    let internal lessonFromDbEntity (u:ReadDb.Db.dataContext.``dbo.LessonsEntity``) =
        {
            Id = u.Id
            Name = u.Name
            Description = u.Description
            StartDate = u.StartDate.ToCzDateTimeOffset()
            EndDate = u.EndDate.ToCzDateTimeOffset()
            Reservations = u.``dbo.LessonReservations by Id`` |> Seq.map reservationFromDbEntity |> Seq.toList
            IsCancelled = u.IsCancelled
        }

    let private getAllForDateRange (ctx:ReadDb.Db.dataContext) (st, en) =
        query {
            for x in ctx.Dbo.Lessons do
            where (x.StartDate >= st && x.StartDate <= en)
            sortBy x.StartDate
            select x
        }
        |> Seq.toList
        |> List.map lessonFromDbEntity

    let createDefault (connString:string) =
        let ctx = ReadDb.Db.GetDataContext(connString)
        {
            GetAllForDateRange = getAllForDateRange ctx
        }

module Workshops =

    type WorkshopsQueries = {
        GetAllForDateRange : (DateTimeOffset * DateTimeOffset) -> Workshop list
    }

    let internal workshopFromDbEntity (u:ReadDb.Db.dataContext.``dbo.WorkshopsEntity``) =
        {
            Id = u.Id
            Name = u.Name
            Description = u.Description
            StartDate = u.StartDate.ToCzDateTimeOffset()
            EndDate = u.EndDate.ToCzDateTimeOffset()
        }

    let private getAllForDateRange (ctx:ReadDb.Db.dataContext) (st, en) =
        query {
            for x in ctx.Dbo.Workshops do
            where (x.StartDate >= st && x.StartDate <= en)
            sortBy x.StartDate
            select x
        }
        |> Seq.toList
        |> List.map workshopFromDbEntity

    let createDefault (connString:string) =
        let ctx = ReadDb.Db.GetDataContext(connString)
        {
            GetAllForDateRange = getAllForDateRange ctx
        }

module MyLessons =
    open Yobo.Shared.MyLessons.Domain
    let private lessonFromDbEntity (u:ReadDb.Db.dataContext.``dbo.LessonsEntity``,credits:bool) =
        {
            Id = u.Id
            Name = u.Name
            Description = u.Description
            StartDate = u.StartDate.ToCzDateTimeOffset()
            EndDate = u.EndDate.ToCzDateTimeOffset()
            CreditsUsed = credits
        } : Lesson

    let getMyLessons (ctx:ReadDb.Db.dataContext) userId =
        query {
            for x in ctx.Dbo.LessonReservations do
            for y in x.``dbo.Lessons by Id`` do
            where (x.UserId = userId && y.EndDate > DateTimeOffset.UtcNow)
            sortBy y.StartDate
            select (y, x.UseCredits)
        }
        |> Seq.toList
        |> List.map lessonFromDbEntity