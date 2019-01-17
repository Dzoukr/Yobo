module Yobo.Core.Lessons.ReadQueries

open Yobo.Core
open System
open FSharp.Rop
open Yobo.Shared.Admin.Domain
open Extensions

type LessonsQueries<'a> = {
    GetAllForDateRange : (DateTime * DateTime) -> Result<Lesson list, 'a>
}

let withError (fn:'a -> 'b) (q:LessonsQueries<'a>) = {
    GetAllForDateRange = q.GetAllForDateRange >> Result.mapError fn
}

let internal lessonFromDbEntity (u:ReadDb.Db.dataContext.``dbo.LessonsEntity``) =
    {
        Id = u.Id
        Name = u.Name
        Description = u.Description
        StartDateUtc = u.StartDateUtc.ToUtc()
        EndDateUtc = u.EndDateUtc.ToUtc()
        Reservations = [] // TODO:
    }

let private getAllForDateRange (st, en) (ctx:ReadDb.Db.dataContext) =
    query {
        for x in ctx.Dbo.Lessons do
        where (x.StartDateUtc >= st && x.StartDateUtc <= en)
        sortBy x.StartDateUtc
        select x
    }
    |> Seq.toList
    |> List.map lessonFromDbEntity

let createDefault (connString:string) =
    let ctx = ReadDb.Db.GetDataContext(connString)
    {
        GetAllForDateRange = getAllForDateRange >> Data.tryQuery ctx
    }