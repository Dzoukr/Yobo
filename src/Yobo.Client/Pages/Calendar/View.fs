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
open Yobo.Shared.Core.Domain.Queries
open Yobo.Shared.Errors
open Yobo.Shared.DateTime
open Yobo.Shared.Core.Reservations.Domain
open Yobo.Shared.Core.Reservations.Domain.Queries

let navigationRow model dispatch =
    Html.tr [
        prop.className "controls"
        prop.children [
            Html.td [
                prop.colSpan 7
                prop.children [
                    if model.WeekOffset - 1 >= Yobo.Shared.Core.Reservations.Domain.minWeekOffset then
                        Bulma.button [
                            prop.onClick (fun _ -> WeekOffsetChanged(model.WeekOffset - 1) |> dispatch)
                            prop.children [ Html.faIconSingle "fas fa-chevron-circle-left" ]
                        ]
                    Bulma.button [
                        prop.onClick (fun _ -> WeekOffsetChanged(0) |> dispatch)
                        prop.children [ Html.faIconSingle "fas fa-home" ]
                    ]
                    Bulma.button [
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

let getTag (la:LessonAvailability) =
    let tagColor, tagText =
        match la with
        | Available Free -> tag.isSuccess, "Volno"
        | Available LastFreeSpot -> tag.isWarning, "Poslední volné místo"
        | Unavailable Full -> tag.isDanger, "Obsazeno"
        | Unavailable AlreadyStarted -> tag.isBlack, "Lekce uzavřena"
        | Unavailable Cancelled -> tag.isBlack, "Lekce zrušena"
    Bulma.tag [ tagColor; prop.text tagText ] 

let isCancelled (la:LessonAvailability) =
    match la with
    | Unavailable Cancelled -> true
    | _ -> false

let getPopoverPosition (d:DateTimeOffset) =
    match d.DayOfWeek with
    | DayOfWeek.Monday -> popover.isRight
    | DayOfWeek.Sunday -> popover.isLeft
    | _ -> popover.isBottom

let lessonDiv dispatch (lesson:Queries.Lesson) =
    let isCancelled = lesson.Availability |> isCancelled
    let position = lesson.StartDate |> getPopoverPosition
    let btns =
        match lesson.ReservationAvailability with
        | Reservable Cash ->
            Bulma.button [
                prop.text "Rezervovat (hotovost)"
            ]
        | Reservable Credits -> 
            Bulma.button [
                prop.text "Rezervovat"
            ]
        | AlreadyReserved (tp, true) ->
            Bulma.button [
                prop.text "Zrušit rezervaci"
            ]
        | AlreadyReserved (_, false)
        | Unreservable -> Html.none
    
    Html.div [
        prop.className "lesson"
        prop.children [
            Popover.popover [
                position
                prop.children [
                    Html.div [
                        prop.className [isCancelled, "cancelled"]
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
                            Html.div [ getTag lesson.Availability ]
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
                        Bulma.buttons [ btns ]
                    ]
                ]
            ]
        ]
    ]
    
    
    
let onlineLessonDiv dispatch (lesson:Queries.OnlineLesson) =
    let isCancelled = lesson.Availability |> isCancelled 
    Html.div [
        prop.className [true, "online-lesson"; isCancelled, "cancelled"]
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
            Html.div [ getTag lesson.Availability ]
            
        ]
    ]    

let col (lessons:Queries.Lesson list) (*workshops:Queries.Workshop list*) (onlines:Queries.OnlineLesson list) (date:DateTimeOffset) dispatch =
    Html.td [
        // Html.div (workshops |> List.map (workshopDiv dispatch))
        Html.div (lessons |> List.map (lessonDiv dispatch))
        Html.div (onlines |> List.map (onlineLessonDiv dispatch))
    ]

let row model dispatch dates =
    let getLessonsForDate (date:DateTimeOffset) =
        model.Lessons
        |> List.filter (fun x -> x.StartDate.Date = date.Date)

//    let getWorkshopsForDate (date:DateTimeOffset) =
//        model.Workshops
//        |> List.filter (fun x -> x.StartDate.Date = date.Date)

    let getOnlineLessonsForDate (date:DateTimeOffset) =
        model.OnlineLessons
        |> List.filter (fun x -> x.StartDate.Date = date.Date)

    dates
    |> List.map (fun date ->
        let lsns = date |> getLessonsForDate
        //let wrksps = date |> getWorkshopsForDate
        let onlns = date |> getOnlineLessonsForDate
        col lsns onlns date dispatch
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
            table.isFullwidth
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