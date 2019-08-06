module Yobo.Core.Lessons.Projections

open System

type UserReservation = {
    UserId : Guid
    Count : int
    UseCredits : bool
}

type ExistingLesson = {
    Id : Guid
    Reservations : UserReservation list
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    IsCancelled : bool
}

type ExistingUser = {
    UserId : Guid
    Credits : int
    CreditsExpiration : DateTimeOffset option
    CashReservationsBlockedUntil : DateTimeOffset option
    IsActivated : bool
}

type ExistingWorkshop = {
    Id : Guid
}

module DbProjections =
    open Yobo.Core.ReadDb

    let private toExistingUser (e:Db.dataContext.``dbo.UsersEntity``) = {
        UserId = e.Id
        Credits = e.Credits
        CreditsExpiration = e.CreditsExpiration
        CashReservationsBlockedUntil = e.CashReservationBlockedUntil
        IsActivated = e.Activated.IsSome
    }

    let private toUserReservation (e:Db.dataContext.``dbo.LessonReservationsEntity``) = {
        UserId = e.UserId
        Count = e.Count
        UseCredits = e.UseCredits
    }

    let private toExistingLesson (e:Db.dataContext.``dbo.LessonsEntity``) = {
        Id = e.Id
        Reservations = e.``dbo.LessonReservations by Id`` |> Seq.map toUserReservation |> Seq.toList
        StartDate = e.StartDate
        EndDate = e.EndDate
        IsCancelled = e.IsCancelled
    }

    let private toExistingWorkshop (e:Db.dataContext.``dbo.WorkshopsEntity``) = {
        Id = e.Id
    }

    let getUserById (ctx:Db.dataContext) i =
        query {
            for x in ctx.Dbo.Users do
            where (x.Id = i)
            select x
        }
        |> Seq.map toExistingUser
        |> Seq.tryHead

    let getLessonById (ctx:Db.dataContext) i =
        query {
            for x in ctx.Dbo.Lessons do
            where (x.Id = i)
            select x
        }
        |> Seq.map toExistingLesson
        |> Seq.tryHead

    let getWorkshopById (ctx:Db.dataContext) i =
        query {
            for x in ctx.Dbo.Workshops do
            where (x.Id = i)
            select x
        }
        |> Seq.map toExistingWorkshop
        |> Seq.tryHead

    let getAllLessons (ctx:Db.dataContext) () =
        query {
            for x in ctx.Dbo.Lessons do
            select x
        }
        |> Seq.map toExistingLesson
        |> Seq.toList

    let getAllWorkshops (ctx:Db.dataContext) () =
        query {
            for x in ctx.Dbo.Workshops do
            select x
        }
        |> Seq.map toExistingWorkshop
        |> Seq.toList