module Yobo.Core.Users.ReadQueries

open Yobo.Core
open System
open FSharp.Rop
open Yobo.Shared.Auth

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

let private getByActivationKey key (ctx:ReadDb.Db.dataContext) =
    query {
        for x in ctx.Dbo.Users do
        where (x.ActivationKey = key)
        select x
    }
    |> Data.oneOrError key
    <!> userFromDbEntity

let private login (verifyHashFn:string -> string -> bool) email pwd (ctx:ReadDb.Db.dataContext) =
    let user =
        query {
            for x in ctx.Dbo.Users do
            where (x.Email = email)
            select x
        } |> Seq.tryHead
    match user with
    | Some u ->
        match (verifyHashFn pwd u.PasswordHash), u.ActivatedUtc with
        | true, Some _ ->  u |> userFromDbEntity |> Ok
        | true, None -> AccountNotActivated(u.Id) |> Error
        | false, _ -> InvalidLoginOrPassword |> Error
    | None -> InvalidLoginOrPassword |> Error

type UserQueries<'a>(connString:string, mapError:Data.DbError -> 'a) =
    let ctx = ReadDb.Db.GetDataContext(connString)
    member __.GetByActivationKey key = key |> getByActivationKey |> Data.tryQuery ctx |> Result.mapError mapError

type Authenticator<'a>(connString:string, mapError:AuthError -> 'a) =
    let ctx = ReadDb.Db.GetDataContext(connString)
    member __.Login verifyHash email pwd =
        login verifyHash email pwd
        |> Data.tryQueryM (fun _ -> InvalidLoginOrPassword) ctx
        |> Result.mapError mapError