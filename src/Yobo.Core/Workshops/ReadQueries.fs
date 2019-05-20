module Yobo.Core.Workshops.ReadQueries

open Yobo.Core
open System
open Yobo.Shared.Domain
open Extensions

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