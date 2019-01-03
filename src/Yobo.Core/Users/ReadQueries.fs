module Yobo.Core.Users.ReadQueries

open Yobo.Core
open System
open FSharp.Rop
open Yobo.Shared.Auth
open Yobo.Core.Data

type User = {
    Id : Guid
    Email : string
    FirstName : string
    LastName : string
}

type UserQueries<'a> = {
    GetByActivationKey : Guid -> Result<User, 'a>
    GetById : Guid -> Result<User, 'a>
}

let withError (fn:'a -> 'b) (q:UserQueries<'a>) = {
    GetByActivationKey = q.GetByActivationKey >> Result.mapError fn
    GetById = q.GetById >> Result.mapError fn
}

let withErrorForActivation (fn:'a -> 'a) q = { q with GetByActivationKey = q.GetByActivationKey >> Result.mapError fn }

let internal userFromDbEntity (u:ReadDb.Db.dataContext.``dbo.UsersEntity``) =
    {
        Id = u.Id
        Email = u.Email
        FirstName = u.FirstName
        LastName = u.LastName
    }

let private getByActivationKey key (ctx:ReadDb.Db.dataContext) =
    query {
        for x in ctx.Dbo.Users do
        where (x.ActivationKey = key)
        select x
    }
    |> Data.oneOrError key
    <!> userFromDbEntity

let private getById i (ctx:ReadDb.Db.dataContext) =
    query {
        for x in ctx.Dbo.Users do
        where (x.Id = i)
        select x
    }
    |> Data.oneOrError i
    <!> userFromDbEntity

let createDefault (connString:string) =
    let ctx = ReadDb.Db.GetDataContext(connString)
    {
        GetByActivationKey = getByActivationKey >> Data.tryQuery ctx
        GetById = getById >> Data.tryQuery ctx
    }