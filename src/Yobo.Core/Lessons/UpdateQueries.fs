module Yobo.Core.Lessons.UpdateQueries

open Yobo.Core
open System

let private getReservationById (ctx:ReadDb.Db.dataContext) userId lessonId =
    query {
        for x in ctx.Dbo.LessonReservations do
        where (x.UserId = userId && x.LessonId = lessonId)
        select x
    } |> Seq.head

let private getReservationsById (ctx:ReadDb.Db.dataContext) lessonId =
    query {
        for x in ctx.Dbo.LessonReservations do
        where (x.LessonId = lessonId)
        select x
    }

let private getById (ctx:ReadDb.Db.dataContext) lessonId =
    query {
        for x in ctx.Dbo.Lessons do
        where (x.Id = lessonId)
        select x
    } |> Seq.head

let private getUserById (ctx:ReadDb.Db.dataContext) userId =
    query {
        for x in ctx.Dbo.Users do
        where (x.Id = userId)
        select x
    } |> Seq.head


let lessonCreated (args:CmdArgs.CreateLesson) (ctx:ReadDb.Db.dataContext) =
    let item = ctx.Dbo.Lessons.Create()
    item.Id <- args.Id
    item.Name <- args.Name
    item.Description <- args.Description
    item.StartDate <- args.StartDate
    item.EndDate <- args.EndDate
    item.Created <- DateTimeOffset.Now
    item.IsCancelled <- false

let reservationAdded (args:CmdArgs.AddReservation) (ctx:ReadDb.Db.dataContext) =
    let item = ctx.Dbo.LessonReservations.Create()
    item.LessonId <- args.Id
    item.UserId <- args.UserId
    item.Count <- args.Count
    item.Created <- DateTimeOffset.Now
    item.UseCredits <- args.UseCredits

let reservationCancelled (args:CmdArgs.CancelReservation) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getReservationById ctx args.UserId
    item.Delete()

let lessonCancelled (args:CmdArgs.CancelLesson) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.IsCancelled <- true
    args.Id 
    |> getReservationsById ctx 
    |> Seq.iter (fun x -> x.Delete())

let creditsAdded (args:CmdArgs.AddCredits) (ctx:ReadDb.Db.dataContext) =
    let item = args.UserId |> getUserById ctx
    item.Credits <- item.Credits + args.Credits
    item.CreditsExpiration <- Some args.Expiration

let creditsWithdrawn (args:CmdArgs.WithdrawCredits) (ctx:ReadDb.Db.dataContext) =
    let item = args.UserId |> getUserById ctx
    item.Credits <- item.Credits - args.Amount

let creditsRefunded (args:CmdArgs.RefundCredits) (ctx:ReadDb.Db.dataContext) =
    let item = args.UserId |> getUserById ctx
    item.Credits <- item.Credits + args.Amount

let cashReservationBlocked (args:CmdArgs.BlockCashReservations) (ctx:ReadDb.Db.dataContext) =
    let item = args.UserId |> getUserById ctx
    item.CashReservationBlockedUntil <- Some args.Expires

let cashReservationUnblocked (args:CmdArgs.UnblockCashReservations) (ctx:ReadDb.Db.dataContext) =
    let item = args.UserId |> getUserById ctx
    item.CashReservationBlockedUntil <- None

let private getWorkshopById (ctx:ReadDb.Db.dataContext) i =
    query {
        for x in ctx.Dbo.Workshops do
        where (x.Id = i)
        select x
    } |> Seq.head

let workshopCreated (args:CmdArgs.CreateWorkshop) (ctx:ReadDb.Db.dataContext) =
    let item = ctx.Dbo.Workshops.Create()
    item.Id <- args.Id
    item.Name <- args.Name
    item.Description <- args.Description
    item.StartDate <- args.StartDate
    item.EndDate <- args.EndDate
    item.Created <- DateTimeOffset.Now

let workshopDeleted (args:CmdArgs.DeleteWorkshop) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getWorkshopById ctx
    item.Delete()