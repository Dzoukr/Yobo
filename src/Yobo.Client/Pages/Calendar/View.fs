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

let getTag (aval:Availability) isCancelled =
    let tagColor =
        match aval with
        | Closed -> tag.isBlack
        | Available Free -> tag.isSuccess
        | Available LastFreeSpot -> tag.isWarning
        | Available Full -> tag.isDanger
        
    let tagText =
        if isCancelled then "Lekce zrušena"
        else
            match aval with
            | Closed -> "Lekce uzavřena"
            | Available Free -> "Volno"
            | Available LastFreeSpot -> "Poslední volné místo"
            | Available Full -> "Obsazeno"
    Bulma.tag [ tagColor; prop.text tagText ] 

let lessonDiv dispatch (lesson:Queries.Lesson) =
    
    Popover.popover [
        popover.isRight
        prop.children [
            Html.div [
                popover.trigger
                ++ prop.className [true, "lesson"; lesson.IsCancelled, "cancelled"]
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
                    Html.div [ getTag lesson.Availability lesson.IsCancelled ]
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
                    
                ]
            ]
        ]
    ]
    
    
let onlineLessonDiv dispatch (lesson:Queries.OnlineLesson) =
      
    Html.div [
        prop.className [true, "online-lesson"; lesson.IsCancelled, "cancelled"]
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
            Html.div [ getTag lesson.Availability lesson.IsCancelled ]
            
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