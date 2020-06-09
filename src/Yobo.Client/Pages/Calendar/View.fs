module Yobo.Client.Pages.Calendar.View

open Domain
open Feliz
open Fable.React
open Feliz.Bulma
open Feliz.Bulma.Operators
open Feliz.Bulma.Popover
open Fable.React.Props
open System
open Yobo.Shared
open Yobo.Client
open Yobo.Client.SharedView
open Yobo.Client.SharedView
open Yobo.Shared.Core.Domain
open Yobo.Shared.Core.Domain.Queries
open Yobo.Shared.Errors
open Yobo.Shared.DateTime
open Yobo.Shared.Core.Reservations.Domain
open Yobo.Shared.Core.Reservations.Domain.Queries
open Yobo.Shared.Core.Reservations.Communication

let navigationRow model dispatch =
    Html.tr [
        prop.className "controls"
        prop.children [
            Html.td [
                prop.colSpan 7
                prop.children [
                    if model.WeekOffset - 1 >= Yobo.Shared.Core.Reservations.Domain.minWeekOffset then
                        Bulma.button.button [
                            prop.onClick (fun _ -> WeekOffsetChanged(model.WeekOffset - 1) |> dispatch)
                            prop.children [ Html.faIconSingle "fas fa-chevron-circle-left" ]
                        ]
                    Bulma.button.button [
                        prop.onClick (fun _ -> WeekOffsetChanged(0) |> dispatch)
                        prop.children [ Html.faIconSingle "fas fa-home" ]
                    ]
                    Bulma.button.button [
                        prop.onClick (fun _ -> WeekOffsetChanged(model.WeekOffset + 1) |> dispatch)
                        prop.children [ Html.faIconSingle "fas fa-chevron-circle-right" ]
                    ]
                ]
            ]
        ]
    ]

let headerTd (date:DateTimeOffset) =
    Html.td [
        Html.div [ prop.className "name"; prop.text (date.DayOfWeek |> DayOfWeek.toCzDay) ]
        Html.div [ prop.className "date"; prop.text (date |> DateTimeOffset.toCzDate) ]
    ]

let headerRow dates =
    dates
    |> List.map headerTd
    |> (fun x -> Html.tr [ prop.className "header"; prop.children x ])

let getTag (la:LessonStatus) =
    let tagColor, tagText =
        match la with
        | Open Free -> color.isSuccess, "Volno"
        | Open LastFreeSpot -> color.isWarning, "Poslední volné místo"
        | Closed Full -> color.isDanger, "Obsazeno"
        | Closed AlreadyStarted -> color.isBlack, "Lekce uzavřena"
        | Closed Cancelled -> color.isBlack, "Lekce zrušena"
    Bulma.tag [ tagColor; prop.text tagText ] 

let isCancelled (la:LessonStatus) =
    match la with
    | Closed Cancelled -> true
    | _ -> false

let getPopoverPosition (d:DateTimeOffset) =
    match d.DayOfWeek with
    | DayOfWeek.Monday -> popover.isRight
    | DayOfWeek.Sunday -> popover.isLeft
    | _ -> popover.isBottom

let lessonDiv dispatch (lesson:Queries.Lesson) =
    let isCancelled = lesson.LessonStatus |> isCancelled
    let position = lesson.StartDate |> getPopoverPosition
    let btns =
        match lesson.ReservationStatus with
        | CanBeReserved LessonPayment.Cash ->
            Bulma.button.button [
                color.isPrimary
                prop.text "Rezervovat (hotovost)"
                prop.onClick (fun _ -> ({ LessonId = lesson.Id; Payment = LessonPayment.Cash } : Request.AddReservation) |> AddReservation |> dispatch )
            ]
        | CanBeReserved LessonPayment.Credits -> 
            Bulma.button.button [
                color.isPrimary
                prop.text "Rezervovat"
                prop.onClick (fun _ -> ({ LessonId = lesson.Id; Payment = LessonPayment.Credits } : Request.AddReservation) |> AddReservation |> dispatch )
            ]
        | Reserved (tp, true) ->
            Bulma.button.button [
                prop.text "Zrušit rezervaci"
            ]
        | Reserved (_, false)
        | Unreservable -> Html.none
    
    Html.div [
        prop.className "lesson"
        prop.children [
            Popover.popover [
                position
                prop.children [
                    Html.div [
                        if isCancelled then prop.className "cancelled"
                        prop.children [
                            Html.div [
                                prop.className "time"
                                prop.children [
                                    Html.text (sprintf "%s - %s" (lesson.StartDate |> DateTimeOffset.toCzTime) (lesson.EndDate |> DateTimeOffset.toCzTime))
                                ]
                            ]
                            Html.div [
                                prop.className "name"
                                prop.text lesson.Name
                            ]
                            Html.div [ getTag lesson.LessonStatus ]
                        ]
                    ]
                    Popover.content [
                        Html.div [
                            prop.className "name"
                            prop.text lesson.Name
                        ]
                        Html.div [
                            prop.className "time"
                            prop.children [
                                Html.faIcon "far fa-clock"
                                Html.text (sprintf "%s od %s do %s" (lesson.StartDate |> DateTimeOffset.toCzDate) (lesson.StartDate |> DateTimeOffset.toCzTime) (lesson.EndDate |> DateTimeOffset.toCzTime))
                            ]
                        ]
                        Html.div lesson.Description
                        Bulma.buttons [
                            buttons.isCentered
                            prop.children btns
                        ]
                    ]
                ]
            ]
        ]
    ]
    
let col (lessons:Queries.Lesson list) (*workshops:Queries.Workshop list*) (date:DateTimeOffset) dispatch =
    Html.td [
        // Html.div (workshops |> List.map (workshopDiv dispatch))
        Html.div (lessons |> List.map (lessonDiv dispatch))
    ]

let row model dispatch dates =
    let getLessonsForDate (date:DateTimeOffset) =
        model.Lessons
        |> List.filter (fun x -> x.StartDate.Date = date.Date)

//    let getWorkshopsForDate (date:DateTimeOffset) =
//        model.Workshops
//        |> List.filter (fun x -> x.StartDate.Date = date.Date)

    dates
    |> List.map (fun date ->
        let lsns = date |> getLessonsForDate
        //let wrksps = date |> getWorkshopsForDate
        col lsns date dispatch
    )
    |> (fun x ->
        Html.tr [
            prop.className "day"
            prop.children x
        ]
    )

let view (model:Model) (dispatch:Msg -> unit) =
    let dates =
        model.WeekOffset
        |> DateRange.getDateRangeForWeekOffset
        |> DateRange.dateRangeToDays
    
    Html.div [
        
        Bulma.table [
            table.isFullWidth
            table.isBordered
            ++ prop.className "table-calendar"
            prop.children [
                Html.tbody [
                    navigationRow model dispatch
                    dates |> headerRow
                    dates |> row model dispatch
                ]
            ]
        ]
    ]