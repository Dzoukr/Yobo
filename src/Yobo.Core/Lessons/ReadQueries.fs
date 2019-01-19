module Yobo.Core.Lessons.ReadQueries

open Yobo.Core
open System
open FSharp.Rop
open Yobo.Shared.Domain

type LessonsQueries<'a> = {
    GetAllForDateRange : (DateTimeOffset * DateTimeOffset) -> Result<Lesson list, 'a>
}

let withError (fn:'a -> 'b) (q:LessonsQueries<'a>) = {
    GetAllForDateRange = q.GetAllForDateRange >> Result.mapError fn
}

let internal lessonFromDbEntity (u:ReadDb.Db.dataContext.``dbo.LessonsEntity``) =
    {
        Id = u.Id
        Name = u.Name
        Description = u.Description
        StartDate = u.StartDate
        EndDate = u.EndDate
        Reservations = [] // TODO:
    }

let private getAllForDateRange (st, en) (ctx:ReadDb.Db.dataContext) =
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
        GetAllForDateRange = getAllForDateRange >> Data.tryQuery ctx
    }