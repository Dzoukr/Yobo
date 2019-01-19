module Yobo.Shared.Calendar.Domain

open System

type Availability =
    | Full
    | LastFreeSpot
    | Free

type UserReservation =
    | ForOne
    | ForTwo
with
    static member FromInt x =
        match x with
        | 1 -> ForOne
        | 2 -> ForTwo
        | _ -> failwith "Only two reservations are allowed"
    member x.ToInt =
        match x with
        | ForOne -> 1
        | ForTwo -> 2

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
            |> Option.map UserReservation.FromInt
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