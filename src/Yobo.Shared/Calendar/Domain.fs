module Yobo.Shared.Calendar.Domain

open System
open Yobo.Shared.Admin.Domain

type Availability =
    | Full
    | LastFreeSpot
    | Free

let maxCapacity = 12

type Lesson = {
    Id : Guid
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    Name : string
    Description : string
    Availability : Availability
    UserReservation : UserReservation option
}
with
    static member FromAdminLesson currentUserId (lesson:Yobo.Shared.Admin.Domain.Lesson) =
        let av =
            match maxCapacity - lesson.Reservations.Length with
            | 1 -> LastFreeSpot
            | x when x >= 2 -> Free
            | _ -> Full
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
        } : Lesson

type AddReservation = {
    LessonId : Guid
    UserReservation : UserReservation
}