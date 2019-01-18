module Yobo.Core.Lessons.UpdateQueries

open Yobo.Core
open System

let private getById (ctx:ReadDb.Db.dataContext) i =
    query {
        for x in ctx.Dbo.Lessons do
        where (x.Id = i)
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