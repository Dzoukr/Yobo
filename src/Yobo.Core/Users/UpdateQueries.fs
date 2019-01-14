module Yobo.Core.Users.UpdateQueries

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
    item.RegisteredUtc <- DateTime.UtcNow
    item.Credits <- 0
    ctx.SubmitUpdates()

let activated (args:CmdArgs.Activate) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.ActivatedUtc <- Some DateTime.UtcNow
    ctx.SubmitUpdates()

let activationKeyRegenerated (args:CmdArgs.RegenerateActivationKey) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.ActivationKey <- args.ActivationKey
    ctx.SubmitUpdates()

let creditsAdded (args:CmdArgs.AddCredits) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.Credits <- item.Credits + args.Credits
    item.CreditsExpirationUtc <- Some args.ExpirationUtc
    ctx.SubmitUpdates()