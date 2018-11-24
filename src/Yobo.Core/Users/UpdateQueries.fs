module Yobo.Core.Users.UpdateQueries

open Yobo.Core
open System

let register (args:CmdArgs.Register) (ctx:ReadDb.Db.dataContext) =
    let item = ctx.Dbo.Users.Create()
    item.Id <- args.Id
    item.Email <- args.Email
    item.FirstName <- args.FirstName
    item.LastName <- args.LastName
    item.ConfirmationKey <- args.ConfirmationKey
    item.PasswordHash <- args.PasswordHash
    item.RegisteredUtc <- DateTime.UtcNow
    ctx.SubmitUpdates()