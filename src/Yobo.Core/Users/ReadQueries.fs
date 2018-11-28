module Yobo.Core.Users.ReadQueries

open Yobo.Core
open System
open FSharp.Rop

type User = {
    Id : Guid
    Email : string
    FirstName : string
    LastName : string
}

let private userFromDbEntity (u:ReadDb.Db.dataContext.``dbo.UsersEntity``) =
    {
        Id = u.Id
        Email = u.Email
        FirstName = u.FirstName
        LastName = u.LastName
    }

let private getUserByActivationKey key (ctx:ReadDb.Db.dataContext) =
    query {
        for x in ctx.Dbo.Users do
        where (x.ActivationKey = key)
        select x
    }
    |> Data.oneOrError key
    <!> userFromDbEntity
    

type UserQueries<'a>(connString:string, mapError:Data.DbError -> 'a) =
    let ctx = ReadDb.Db.GetDataContext(connString)
    member __.GetUserByActivationKey key = key |> getUserByActivationKey |> Data.tryQuery ctx |> Result.mapError mapError