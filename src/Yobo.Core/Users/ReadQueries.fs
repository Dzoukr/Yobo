module Yobo.Core.Users.ReadQueries

open Yobo.Core
open System
open Yobo.Shared.Domain
open Extensions

type UserQueries = {
    GetById : Guid -> User option
    GetAll : unit -> User list
    GetByEmail : string -> User option
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

let private getById (ctx:ReadDb.Db.dataContext) i =
    query {
        for x in ctx.Dbo.Users do
        where (x.Id = i)
        select x
    }
    |> Seq.map userFromDbEntity
    |> Seq.tryHead

let private getByEmail (ctx:ReadDb.Db.dataContext) (e:string) =
    query {
        for x in ctx.Dbo.Users do
        where (x.Email.ToLower() = e.ToLower())
        select x
    }
    |> Seq.map userFromDbEntity
    |> Seq.tryHead

let private getAll (ctx:ReadDb.Db.dataContext) () =
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
        GetById = getById ctx
        GetAll = getAll ctx
        GetByEmail = getByEmail ctx
    }