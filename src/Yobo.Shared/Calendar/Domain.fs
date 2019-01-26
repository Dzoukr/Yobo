module Yobo.Shared.Calendar.Domain

open System
open Yobo.Shared.Domain
open Yobo.Shared.Extensions

type Availability =
    | LastFreeSpot
    | Free

let maxCapacity = 12

let getCancellingDate (d:DateTimeOffset) =
    d.StartOfTheDay().AddHours 9.

type Lesson = {
    Id : Guid
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    Name : string
    Description : string
    Availability : Availability option
    UserReservation : UserReservation option
    CancellableUntil : DateTimeOffset
    IsCancelled : bool
}
with
    static member FromAdminLesson currentUserId (lesson:Yobo.Shared.Domain.Lesson) =
        let av =
            match maxCapacity - lesson.Reservations.Length with
            | 1 -> LastFreeSpot |> Some
            | x when x >= 2 -> Free |> Some
            | _ -> None
        let ur =
            lesson.Reservations
            |> List.tryFind (fun (u,_) -> u.Id = currentUserId)
            |> Option.map snd
        {
            Id = lesson.Id
            StartDate = lesson.StartDate
            EndDate = lesson.EndDate
            Name = lesson.Name
            Description = lesson.Description
            Availability = av
            UserReservation = ur
            CancellableUntil = lesson.StartDate |> getCancellingDate
            IsCancelled = lesson.IsCancelled
        } : Lesson

type AddReservation = {
    LessonId : Guid
    UserReservation : UserReservation
}