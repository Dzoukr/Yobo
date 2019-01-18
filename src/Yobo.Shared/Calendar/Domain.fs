module Yobo.Shared.Calendar.Domain

open System

type Availability =
    | Full
    | LastFreeSpot
    | Free

type Lesson = {
    Id : Guid
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    Name : string
    Description : string
    Availability : Availability
    UserReservations : int
}
with
    static member FromAdminLesson currentUserId (lesson:Yobo.Shared.Admin.Domain.Lesson) =
        let av =
            match 12 - lesson.Reservations.Length with
            | 1 -> LastFreeSpot
            | x when x >= 2 -> Free
            | _ -> Full
        let ur =
            lesson.Reservations
            |> List.tryFind (fun (u,x) -> u.Id = currentUserId)
            |> Option.map snd
            |> Option.defaultValue 0
        {
            Id = lesson.Id
            StartDate = lesson.StartDate
            EndDate = lesson.EndDate
            Name = lesson.Name
            Description = lesson.Description
            Availability = av
            UserReservations = ur
        } : Lesson