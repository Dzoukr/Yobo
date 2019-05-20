module Yobo.Core.Users.Authenticator

open Yobo.Core
open System
open FSharp.Rop
open Yobo.Shared.Auth
open Yobo.Shared.Domain

type Authenticator = {
    Login : string -> string -> Result<User,AuthError>
    GetByActivationKey : Guid -> User option
    GetByEmail : string -> User option
    GetByPasswordResetKey : Guid -> User option
}

let private getByActivationKey (ctx:ReadDb.Db.dataContext) key =
    query {
        for x in ctx.Dbo.Users do
        where (x.ActivationKey = key)
        select x
    }
    |> Seq.tryHead
    |> Option.map ReadQueries.userFromDbEntity

let private getByPasswordResetKey (ctx:ReadDb.Db.dataContext) key=
    query {
        for x in ctx.Dbo.Users do
        where (x.PasswordResetKey = Some key)
        select x
    }
    |> Seq.tryHead
    |> Option.map ReadQueries.userFromDbEntity


let private getByEmail (ctx:ReadDb.Db.dataContext) (e:string) =
    query {
        for x in ctx.Dbo.Users do
        where (x.Email.ToLower() = e.ToLower())
        select x
    }
    |> Seq.tryHead
    |> Option.map ReadQueries.userFromDbEntity

let private login (ctx:ReadDb.Db.dataContext) (verifyHashFn:string -> string -> bool) email pwd =
    let user =
        query {
            for x in ctx.Dbo.Users do
            where (x.Email = email)
            select x
        } |> Seq.tryHead
    match user with
    | Some u ->
        match (verifyHashFn pwd u.PasswordHash), u.Activated with
        | true, Some _ ->  u |> ReadQueries.userFromDbEntity |> Ok
        | true, None -> AccountNotActivated(u.Id) |> Error
        | false, _ -> InvalidLoginOrPassword |> Error
    | None -> InvalidLoginOrPassword |> Error

let createDefault (connString:string) (verifyHashFn:string -> string -> bool) =
    let ctx = ReadDb.Db.GetDataContext(connString)
    {
        Login = fun l p -> login ctx verifyHashFn l p // |> Data.tryQueryResultM (fun _ -> InvalidLoginOrPassword) ctx
        GetByActivationKey = getByActivationKey ctx // >> Data.tryQueryResult ctx >> Result.mapError (fun _ -> AuthError.ActivationKeyDoesNotMatch)
        GetByEmail = getByEmail ctx // >> Data.tryQueryResult ctx  >> Result.mapError (fun _ -> AuthError.InvalidLogin)
        GetByPasswordResetKey = getByPasswordResetKey ctx //>> Data.tryQueryResult ctx >> Result.mapError (fun _ -> AuthError.PasswordResetKeyDoesNotMatch)
    }