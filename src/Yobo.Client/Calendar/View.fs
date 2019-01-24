module Yobo.Client.Calendar.View

open Domain
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open System
open Yobo.Shared
open Yobo.Client
open Yobo.Shared.Domain

module Calendar =
    open Yobo.Shared.Calendar.Domain

    let lessonDiv (user:User) dispatch (lesson:Lesson) =
        let lessonAlreadyStarted = DateTimeOffset.Now > lesson.StartDate
        let lessonCancellingDate = lesson.StartDate |> Yobo.Shared.Calendar.Domain.getCancellingDate
        let lessonCancellingClosed = lessonAlreadyStarted || DateTimeOffset.Now > lessonCancellingDate

        let st = lesson.StartDate |> SharedView.toCzTime
        let en = lesson.EndDate |> SharedView.toCzTime
        let avail =
            if lessonAlreadyStarted then
                match lesson.UserReservation with
                | Some(ForTwo) -> Tag.tag [ Tag.Color IsInfo ] [ str "Zůčastnili jste + 1" ]
                | Some(ForOne _) -> Tag.tag [ Tag.Color IsInfo ] [ str "Zůčastnili jste" ]
                | None -> str ""
            else
                match lesson.UserReservation with
                | None ->
                    match lesson.Availability with
                    | Free -> Tag.tag [ Tag.Color IsSuccess ] [ str "Volno" ]
                    | LastFreeSpot -> Tag.tag [ Tag.Color IsWarning ] [ str "Poslední volné místo" ]
                    | Full -> Tag.tag [ Tag.Color IsDanger ] [ str "Lekce je již obsazena" ]
                | Some(ForTwo) -> Tag.tag [ Tag.Color IsInfo ] [ str "Rezervováno pro vás + 1" ]
                | Some(ForOne _) -> Tag.tag [ Tag.Color IsInfo ] [ str "Rezervováno pro vás" ]

        let bookBtn =

            let items =
                let forOneCash =
                    Button.button [ Button.Color IsPrimary; Button.Option.Props [ OnClick (fun _ -> { LessonId = lesson.Id; UserReservation = ForOne(Payment.Cash) } |> AddReservation |> dispatch ) ] ]
                        [ str "Rezervovat (platba v hotovosti)" ]
                let forOne =
                    Button.button [ Button.Color IsPrimary; Button.Option.Props [ OnClick (fun _ -> { LessonId = lesson.Id; UserReservation = ForOne(Payment.Credits) } |> AddReservation |> dispatch ) ] ]
                        [ str "Rezervovat" ]

                match user.Credits, user.CashReservationBlockedUntil, lesson.Availability with
                | 0, Some d, Free | 0, Some d, LastFreeSpot ->
                    if DateTimeOffset.Now > d then [ forOneCash ] else []
                | 0, None, Free | 0, None, LastFreeSpot -> [ forOneCash ]
                | 1, _, Free | 1, _, LastFreeSpot -> [ forOne ]
                | _, _, Free -> [ forOne ] //[ forOne; forTwo ]
                | _, _, LastFreeSpot -> [ forOne ]
                | _, _, Full -> []

            let isBtnVisible =
                items.Length > 0
                && not user.IsAdmin
                && lesson.UserReservation.IsNone
                && not lessonAlreadyStarted

            if isBtnVisible then div [] items
            else str ""

        let cancelBtn =
            if lesson.UserReservation.IsSome && not lessonCancellingClosed then
                Button.button [ Button.Color IsDanger; Button.Props [ OnClick (fun _ -> CancelReservation(lesson.Id) |> dispatch) ] ] [
                    str "Zrušit rezervaci"
                ]
            else
                str ""

        let warning =
            if lessonAlreadyStarted then
                "Lekce již proběhla" |> str |> SharedView.infoBox
            else
                if lessonCancellingClosed && lesson.UserReservation.IsSome then
                    "Odhlašování z lekce je již zavřeno" |> str |> SharedView.infoBox
                else if lessonCancellingClosed && lesson.UserReservation.IsNone then
                    "Odhlašování z lekce je již zavřeno. Lze se pouze přihlašovat." |> str |> SharedView.infoBox
                else str ""

        let popoverClass =
            match lesson.StartDate.DayOfWeek with
            | DayOfWeek.Monday -> "is-popover-right"
            | DayOfWeek.Sunday -> "is-popover-left"
            | _ -> "is-popover-bottom"

        div [ ClassName (sprintf "popover %s" popoverClass) ] [
            div [ ClassName "popover-trigger lesson" ] [

                div [ ClassName "time" ] [
                    lesson.StartDate |> SharedView.toCzTime |> str
                    str " - "
                    lesson.EndDate |> SharedView.toCzTime |> str

                ]
                div [ ClassName "name"] [ str lesson.Name ]
                div [ ClassName "availability" ] [ avail ]
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
                warning
                div [] [
                    bookBtn
                ]
                div [] [
                    cancelBtn
                ]
            ]
        ]

    let navigation (state:State) dispatch =
        tr [ ClassName "controls" ] [
            td [ ColSpan 7 ] [
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
        td [ ] [
            div [ ClassName "name" ] [ str n ]
            div [ ClassName "date" ] [ date.ToString("dd. MM. yyyy") |> str ]
        ]

    let col user (lessons:Lesson list) (dispatch : Msg -> unit) (date:DateTimeOffset) =
        td [ ] [
            div [] (lessons |> List.map (lessonDiv user dispatch))
        ]

    let render user (state : State) (dispatch : Msg -> unit) (startDate:DateTimeOffset, endDate:DateTimeOffset) =
        let dates = DateRange.dateRangeToDays(startDate, endDate)
        let getLessonsForDate (date:DateTimeOffset) =
            state.Lessons
            |> List.filter (fun x -> x.StartDate.Date = date.Date)

        let headerRow = dates |> List.map headerCol |> tr [ ClassName "header" ] 
        let row =
            dates
            |> List.map (fun x ->
                let lsns = x |> getLessonsForDate
                col user lsns dispatch x
            )
            |> tr [ ClassName "day" ] 

        Table.table [ Table.CustomClass "is-fullwidth is-bordered table-calendar" ] [
            yield navigation state dispatch
            yield headerRow
            yield row
        ]
    

let render user (state : State) (dispatch : Msg -> unit) =
    div [] [
        Calendar.render user state dispatch (DateRange.getDateRangeForWeekOffset state.WeekOffset)
    ]