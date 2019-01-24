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

    type private BookAllowed =
        | Cash
        | Credit

    type private LessonState =
        | Cancelled
        | AlreadyStarted of userBooked:bool
        | Reserved of cancelAllowed:bool
        | NotReserved of Availability option * bookAllowed:BookAllowed option * cancelAllowed:bool

    let private toLessonState (user:User) (lesson:Lesson) =
        let alreadyStarted = DateTimeOffset.Now > lesson.StartDate
        let cancellingDate = lesson.StartDate |> Yobo.Shared.Calendar.Domain.getCancellingDate
        let cancellingClosed = alreadyStarted || DateTimeOffset.Now > cancellingDate

        let bookAllowed =
            match lesson.Availability with
            | None -> None
            | Some _ ->
                match user.Credits, user.CashReservationBlockedUntil with
                | 0, Some d -> if DateTimeOffset.Now > d then Some Cash else None
                | 0, None -> Some Cash
                | x, _ when x > 0 -> Some Credit
                | _ -> None

        if lesson.IsCancelled then LessonState.Cancelled
        else if alreadyStarted then LessonState.AlreadyStarted(lesson.UserReservation.IsSome)
        else if lesson.UserReservation.IsSome then LessonState.Reserved(not cancellingClosed)
        else NotReserved(lesson.Availability, bookAllowed, not cancellingClosed)

    let lessonDiv (user:User) dispatch (lesson:Lesson) =

        let lessonState = lesson |> toLessonState user
        
        let st = lesson.StartDate |> SharedView.toCzTime
        let en = lesson.EndDate |> SharedView.toCzTime

        let avail =
            match lessonState with
            | Cancelled -> Tag.tag [ Tag.Color IsBlack ] [ str "Lekce je zrušena" ]
            | AlreadyStarted(true) -> Tag.tag [ Tag.Color IsInfo ] [ str "Zůčastnili jste se" ]
            | AlreadyStarted(false) -> str ""
            | Reserved _ -> Tag.tag [ Tag.Color IsInfo ] [ str "Rezervováno pro vás" ]
            | NotReserved(Some Free, _, _) -> Tag.tag [ Tag.Color IsSuccess ] [ str "Volno" ]
            | NotReserved(Some LastFreeSpot, _, _) -> Tag.tag [ Tag.Color IsWarning ] [ str "Poslední volné místo" ]
            | NotReserved(None, _, _) -> Tag.tag [ Tag.Color IsDanger ] [ str "Lekce je již obsazena" ]

        let bookBtn =
            match user.IsAdmin, lessonState with
            | false, NotReserved(_,Some Cash,_) -> 
                Button.button [ Button.Color IsPrimary; Button.Option.Props [ OnClick (fun _ -> { LessonId = lesson.Id; UserReservation = ForOne(Payment.Cash) } |> AddReservation |> dispatch ) ] ]
                        [ str "Rezervovat (platba v hotovosti)" ]
            | false, NotReserved(_, Some Credit,_) ->
                Button.button [ Button.Color IsPrimary; Button.Option.Props [ OnClick (fun _ -> { LessonId = lesson.Id; UserReservation = ForOne(Payment.Credits) } |> AddReservation |> dispatch ) ] ]
                    [ str "Rezervovat" ]
            | _ -> str ""

        let cancelBtn =
            match lessonState with
            | Reserved(true) ->
                Button.button [ Button.Color IsDanger; Button.Props [ OnClick (fun _ -> CancelReservation(lesson.Id) |> dispatch) ] ] [
                    str "Zrušit rezervaci"
                ]
            | _ -> str ""

        let warning =
            match lessonState with
            | AlreadyStarted _ -> "Lekce již proběhla" |> str |> SharedView.infoBox
            | _ -> str ""
            
        let popoverClass =
            match lesson.StartDate.DayOfWeek with
            | DayOfWeek.Monday -> "is-popover-right"
            | DayOfWeek.Sunday -> "is-popover-left"
            | _ -> "is-popover-bottom"

        let cancelledClass = if lesson.IsCancelled then "cancelled" else ""

        div [ ClassName (sprintf "popover %s" popoverClass) ] [
            div [ ClassName (sprintf "popover-trigger lesson %s" cancelledClass) ] [

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