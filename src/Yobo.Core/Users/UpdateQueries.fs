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
    item.Registered <- DateTimeOffset.Now
    item.Credits <- 0
    item.CreditsExpiration <- None
    item.CashReservationBlockedUntil <- None
    ctx.SubmitUpdates()

let activated (args:CmdArgs.Activate) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.Activated <- Some DateTimeOffset.Now
    ctx.SubmitUpdates()

let passwordResetInitiated (args:CmdArgs.InitiatePasswordReset) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.PasswordResetKey <- Some args.PasswordResetKey
    ctx.SubmitUpdates()

let activationKeyRegenerated (args:CmdArgs.RegenerateActivationKey) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.ActivationKey <- args.ActivationKey
    ctx.SubmitUpdates()

let creditsAdded (args:CmdArgs.AddCredits) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.Credits <- item.Credits + args.Credits
    item.CreditsExpiration <- Some args.Expiration
    ctx.SubmitUpdates()

let creditsWithdrawn (args:CmdArgs.WithdrawCredits) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.Credits <- item.Credits - args.Amount
    ctx.SubmitUpdates()

let creditsRefunded (args:CmdArgs.RefundCredits) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.Credits <- item.Credits + args.Amount
    ctx.SubmitUpdates()

let cashReservationBlocked (args:CmdArgs.BlockCashReservations) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.CashReservationBlockedUntil <- Some args.Expires
    ctx.SubmitUpdates()

let cashReservationUnblocked (args:CmdArgs.UnblockCashReservations) (ctx:ReadDb.Db.dataContext) =
    let item = args.Id |> getById ctx
    item.CashReservationBlockedUntil <- None
    ctx.SubmitUpdates()