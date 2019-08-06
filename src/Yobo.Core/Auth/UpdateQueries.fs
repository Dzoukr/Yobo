module Yobo.Core.Auth.UpdateQueries

open Yobo.Core
open System

let private getById (ctx:ReadDb.Db.dataContext) i =
    query {
        for x in ctx.Dbo.Users do
        where (x.Id = i)
        select x
    } |> Seq.head

let registered (args:CmdArgs.Register) (ctx:ReadDb.Db.dataContext) =
    let item = ctx.Dbo.Users.Create()
    item.Id <- args.Id
    item.Email <- args.Email
    item.FirstName <- args.FirstName
    item.LastName <- args.LastName
    item.ActivationKey <- args.ActivationKey
    item.PasswordHash <- args.PasswordHash
    item.Registered <- DateTimeOffset.Now
    item.Credits <- 0
    item.CreditsExpiration <- None
    item.CashReservationBlockedUntil <- None

let activated (args:CmdArgs.Activate) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.Activated <- Some DateTimeOffset.Now

let passwordResetInitiated (args:CmdArgs.InitiatePasswordReset) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.PasswordResetKey <- Some args.PasswordResetKey

let passwordReset (args:CmdArgs.ResetPassword) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.PasswordResetKey <- None
    item.PasswordHash <- args.PasswordHash

let activationKeyRegenerated (args:CmdArgs.RegenerateActivationKey) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.ActivationKey <- args.ActivationKey