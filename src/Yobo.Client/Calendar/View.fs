module Yobo.Client.Calendar.View

open Domain
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open System
open Yobo.Shared
open Yobo.Client

module Calendar =
    open Yobo.Shared.Calendar.Domain

    let lessonDiv dispatch (lesson:Lesson) =
        let st = lesson.StartDate |> SharedView.toCzTime
        let en = lesson.EndDate |> SharedView.toCzTime
        let avail =
            match lesson.UserReservation with
            | None ->
                match lesson.Availability with
                | Free -> Tag.tag [ Tag.Color IsSuccess ] [ str "Volné místo" ]
                | LastFreeSpot -> Tag.tag [ Tag.Color IsWarning ] [ str "Poslední volné místo" ]
                | Full -> Tag.tag [ Tag.Color IsDanger ] [ str "Lekce je již plná" ]
            | Some(ForTwo) -> Tag.tag [ Tag.Color IsInfo ] [ str "Rezervováno pro vás + 1" ]
            | Some(ForOne) -> Tag.tag [ Tag.Color IsInfo ] [ str "Rezervováno pro vás" ]

        let bookBtn =
            let items =
                let forOne = { LessonId = lesson.Id; UserReservation = ForOne }
                let forTwo = { LessonId = lesson.Id; UserReservation = ForTwo }
                match lesson.Availability with
                | Free ->
                    [ Dropdown.Item.a [ Dropdown.Item.Option.Props [ OnClick (fun _ -> forOne |> AddReservation |> dispatch ) ] ] [ str "Zarezervovat místo jen pro mě" ]
                      Dropdown.Item.a [ Dropdown.Item.Option.Props [ OnClick (fun _ -> forTwo |> AddReservation |> dispatch ) ] ] [ str "Přivedu s sebou kamaráda/ku" ] ]
                | LastFreeSpot -> [ Dropdown.Item.a [ Dropdown.Item.Option.Props [ OnClick (fun _ -> forOne |> AddReservation |> dispatch ) ] ] [ str "Zarezervovat místo jen pro mě" ] ]
                | Full -> []

            let isBtnVisible =
                match lesson.UserReservation, lesson.Availability with
                | None, LastFreeSpot | None, Free -> true
                | _ -> false

            if isBtnVisible then
                Dropdown.dropdown [ Dropdown.IsHoverable ] [
                    div [ ] [
                        Button.button [ ] [
                            span [ ] [ str "Rezervace" ]
                            Icon.icon [ Icon.Size IsSmall ] [ i [ ClassName "fas fa-angle-down" ] [ ] ]
                        ]
                    ]
                    Dropdown.menu [ ] [
                        Dropdown.content [ ] items
                    ]
                ]
            else str ""

        div [ ClassName "popover is-popover-bottom"] [
            div [ ClassName "popover-trigger" ] [
                str lesson.Name
                avail
            ]
            div [ ClassName "popover-content" ] [
                div [] [
                    i [ ClassName "fas fa-clock" ] []
                    sprintf "%s - %s" st en |> str
                    avail
                ]
                div [] [
                    str lesson.Name
                ]
                hr []
                div [] [
                    str lesson.Description
                ]
                div [] [
                    bookBtn
                ]
            ]
        ]

    let navigation (state:State) dispatch =
        Columns.columns [] [
            Column.column [ ] [
                Button.button [ Button.Props [ OnClick (fun _ -> WeekOffsetChanged(state.WeekOffset - 1) |> dispatch) ] ] [
                    i [ ClassName "fas fa-chevron-circle-left" ] [ ]
                ]
                Button.button [ Button.Props [ OnClick (fun _ -> WeekOffsetChanged(0) |> dispatch) ] ] [
                    i [ ClassName "fas fa-home" ] [ ]
                ]
                Button.button [ Button.Props [ OnClick (fun _ -> WeekOffsetChanged(state.WeekOffset + 1) |> dispatch) ] ] [
                    i [ ClassName "fas fa-chevron-circle-right" ] [ ]
                ]
            ]
        ]

    let headerCol (date:DateTimeOffset) =
        let n =
            match date.DayOfWeek with
            | DayOfWeek.Monday -> "Pondělí"
            | DayOfWeek.Tuesday -> "Úterý"
            | DayOfWeek.Wednesday -> "Středa"
            | DayOfWeek.Thursday -> "Čtvrtek"
            | DayOfWeek.Friday -> "Pátek"
            | DayOfWeek.Saturday -> "Sobota"
            | DayOfWeek.Sunday -> "Neděle"
            | _ -> ""
        Column.column [] [
            div [] [ str n ]
            div [] [ date.ToString("dd. MM. yyyy") |> str ]
        ]

    let col (lessons:Lesson list) (dispatch : Msg -> unit) (date:DateTimeOffset) =
        Column.column [ ] [
            div [] (lessons |> List.map (lessonDiv dispatch))
        ]

    let render (state : State) (dispatch : Msg -> unit) (startDate:DateTimeOffset, endDate:DateTimeOffset) =
        let dates = DateRange.dateRangeToDays(startDate, endDate)
        let getLessonsForDate (date:DateTimeOffset) =
            state.Lessons
            |> List.filter (fun x -> x.StartDate.Date = date.Date)

        let headerRow = dates |> List.map headerCol |> Columns.columns [] 
        let row =
            dates
            |> List.map (fun x ->
                let lsns = x |> getLessonsForDate
                col lsns dispatch x
            )
            |> Columns.columns [] 

        div [] [
            yield navigation state dispatch
            yield headerRow
            yield row
        ]
    

let render (state : State) (dispatch : Msg -> unit) =
    div [] [
        Calendar.render state dispatch (DateRange.getDateRangeForWeekOffset state.WeekOffset)
    ]