module Yobo.Core.Users.ReadQueries

open Yobo.Core
open System
open FSharp.Rop
open Yobo.Shared.Domain
open Extensions

type UserQueries<'a> = {
    GetById : Guid -> Result<User, 'a>
    GetAll : unit -> Result<User list, 'a>
}

let withError (fn:'a -> 'b) (q:UserQueries<'a>) = {
    GetById = q.GetById >> Result.mapError fn
    GetAll = q.GetAll >> Result.mapError fn
}

let internal userFromDbEntity (u:ReadDb.Db.dataContext.``dbo.UsersEntity``) =
    {
        Id = u.Id
        Email = u.Email
        FirstName = u.FirstName
        LastName = u.LastName
        Activated = u.Activated
        Credits = u.Credits
        CreditsExpiration = u.CreditsExpiration |> Option.map (fun x -> x.ToCzDateTimeOffset())
        CashReservationBlockedUntil = u.CashReservationBlockedUntil |> Option.map (fun x -> x.ToCzDateTimeOffset())
        IsAdmin = false
    }

let private getById i (ctx:ReadDb.Db.dataContext) =
    query {
        for x in ctx.Dbo.Users do
        where (x.Id = i)
        select x
    }
    |> Data.oneOrError i
    <!> userFromDbEntity

let private getAll () (ctx:ReadDb.Db.dataContext) =
    query {
        for x in ctx.Dbo.Users do
        sortBy x.LastName
        select x
    }
    |> Seq.map userFromDbEntity
    |> Seq.toList

let createDefault (connString:string) =
    let ctx = ReadDb.Db.GetDataContext(connString)
    {
        GetById = getById >> Data.tryQueryResult ctx
        GetAll = getAll >> Data.tryQuery ctx
    }