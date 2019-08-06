module Yobo.Core.Auth.Authenticator

open Yobo.Core
open Yobo.Shared.Auth
open Yobo.Shared.Domain

type Authenticator = {
    Login : string -> string -> Result<User,AuthError>
}

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
        Login = fun l p -> login ctx verifyHashFn l p
    }