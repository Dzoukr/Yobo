module Yobo.Core.Users.Authenticator

open Yobo.Core
open System
open FSharp.Rop
open Yobo.Shared.Auth
open Yobo.Core.Data
open ReadQueries

type Authenticator<'a> = {
    Login : string -> string -> Result<User, 'a>
    GetByActivationKey : Guid -> Result<User, 'a>
}

let withError (fn:'a -> 'b) (q:Authenticator<'a>) = {
    Login = fun l p -> q.Login l p |> Result.mapError fn
    GetByActivationKey = q.GetByActivationKey >> Result.mapError fn
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

let createDefault (connString:string) (verifyHashFn:string -> string -> bool) =
    let ctx = ReadDb.Db.GetDataContext(connString)
    {
        Login = fun l p -> login verifyHashFn l p |> Data.tryQueryM (fun _ -> InvalidLoginOrPassword) ctx
        GetByActivationKey = getByActivationKey >> Data.tryQuery ctx >> Result.mapError (fun _ -> ActivationKeyDoesNotMatch)
    }