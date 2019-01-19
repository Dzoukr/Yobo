module Yobo.Core.Lessons.UpdateQueries

open Yobo.Core
open System

let private getById (ctx:ReadDb.Db.dataContext) userId lessonId =
    query {
        for x in ctx.Dbo.LessonReservations do
        where (x.UserId = userId && x.LessonId = lessonId)
        select x
    } |> Seq.head

let created (args:CmdArgs.Create) (ctx:ReadDb.Db.dataContext) =
    let item = ctx.Dbo.Lessons.Create()
    item.Id <- args.Id
    item.Name <- args.Name
    item.Description <- args.Description
    item.StartDate <- args.StartDate
    item.EndDate <- args.EndDate
    item.Created <- DateTimeOffset.Now
    ctx.SubmitUpdates()

let reservationAdded (args:CmdArgs.AddReservation) (ctx:ReadDb.Db.dataContext) =
    let item = ctx.Dbo.LessonReservations.Create()
    item.LessonId <- args.Id
    item.UserId <- args.UserId
    item.Count <- args.Count
    item.Created <- DateTimeOffset.Now
    item.UseCredits <- args.UseCredits
    ctx.SubmitUpdates()

let reservationCancelled (args:CmdArgs.CancelReservation) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx args.UserId
    item.Delete()
    ctx.SubmitUpdates()