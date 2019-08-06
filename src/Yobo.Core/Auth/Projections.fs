module Yobo.Core.Auth.Projections

open System

type ExistingUser = {
    Id : Guid
    Email : string
    IsActivated : bool
    ActivationKey : Guid
    PasswordResetKey : Guid option
}

module DbProjections =
    open Yobo.Core.ReadDb

    let private toExistingUser (e:Db.dataContext.``dbo.UsersEntity``) = {
        Id = e.Id
        Email = e.Email
        IsActivated = e.Activated.IsSome
        ActivationKey = e.ActivationKey
        PasswordResetKey = e.PasswordResetKey
    }

    let getById (ctx:Db.dataContext) i =
        query {
            for x in ctx.Dbo.Users do
            where (x.Id = i)
            select x
        }
        |> Seq.map toExistingUser
        |> Seq.tryHead

    let getByEmail (ctx:Db.dataContext) e =
        query {
            for x in ctx.Dbo.Users do
            where (x.Email = e)
            select x
        }
        |> Seq.map toExistingUser
        |> Seq.tryHead

    let getAll (ctx:Db.dataContext) () =
        query {
            for x in ctx.Dbo.Users do
            select x
        }
        |> Seq.map toExistingUser
        |> Seq.toList

    let getByActivationKey (ctx:Db.dataContext) key =
        query {
               for x in ctx.Dbo.Users do
               where (x.ActivationKey = key)
               select x
        }
        |> Seq.tryHead
        |> Option.map toExistingUser

    let getByPasswordResetKey (ctx:Db.dataContext) key =
        query {
               for x in ctx.Dbo.Users do
               where (x.PasswordResetKey = Some key)
               select x
        }
        |> Seq.tryHead
        |> Option.map toExistingUser