module Yobo.Client.Calendar.View

open Domain
open Fable.React
open Fable.React.Props
open Fulma
open System
open Yobo.Shared
open Yobo.Client
open Yobo.Shared.Domain

module Calendar =
    open Yobo.Shared.Calendar.Domain

    type private LessonState =
        | Cancelled
        | AlreadyStarted of userBooked:bool
        | Reserved of typ:Payment * cancelAllowed:bool
        | NotReserved of Availability option * bookAllowed:Payment option * cancelAllowed:bool

    let private toLessonState (user:User) (lesson:Lesson) =
        let alreadyStarted = DateTimeOffset.Now > lesson.StartDate
        let cancellingDate = lesson.StartDate |> Yobo.Shared.Calendar.Domain.getCancellingDate
        let cancellingClosed = alreadyStarted || DateTimeOffset.Now > cancellingDate
        let getReservationType = function
            | ForOne t -> t
            | ForTwo -> Credits


        let bookAllowed =
            match lesson.Availability with
            | None -> None
            | Some _ ->
                match user.Credits, user.CashReservationBlockedUntil with
                | 0, Some d -> if DateTimeOffset.Now > d then Some Cash else None
                | 0, None -> Some Cash
                | x, _ when x > 0 -> Some Credits
                | _ -> None

        if lesson.IsCancelled then LessonState.Cancelled
        else if alreadyStarted then LessonState.AlreadyStarted(lesson.UserReservation.IsSome)
        else if lesson.UserReservation.IsSome then LessonState.Reserved(getReservationType lesson.UserReservation.Value, not cancellingClosed)
        else NotReserved(lesson.Availability, bookAllowed, not cancellingClosed)

    let workshopDiv (workshop:Workshop) =
        
        let st = workshop.StartDate |> SharedView.toCzTime
        let en = workshop.EndDate |> SharedView.toCzTime
        
        let popoverClass =
            match workshop.StartDate.DayOfWeek with
            | DayOfWeek.Monday -> "is-popover-right"
            | DayOfWeek.Sunday -> "is-popover-left"
            | _ -> "is-popover-bottom"
        
        div [ ClassName (sprintf "popover %s" popoverClass) ] [
            div [ ClassName ("popover-trigger lesson workshop") ] [

                div [ ClassName "time" ] [
                    workshop.StartDate |> SharedView.toCzTime |> str
                    str " - "
                    workshop.EndDate |> SharedView.toCzTime |> str

                ]
                div [ ClassName "name"] [ str workshop.Name ]
            ]
            div [ ClassName "popover-content" ] [
                div [ ClassName "name"] [
                    str workshop.Name
                ]
                div [ ClassName "time"] [
                    i [ ClassName "far fa-clock"; Style [ MarginRight 5 ] ] []
                    sprintf "%s od %s do %s" (workshop.StartDate |> SharedView.toCzDate)  st en |> str
                ]
                div [ DangerouslySetInnerHTML { __html = workshop.Description.Replace("\n","<br/>") } ] [ ]
                div [ ClassName "name"] [
                    str "Přihlášení přes email: "
                    a [ Href "mailto:jana@mindfulyoga.cz"] [ str "jana@mindfulyoga.cz"] 
                    str " nebo na tel. čísle 777 835 160"
                ]
            ]
        ]
            
    let lessonDiv (user:User) dispatch (lesson:Lesson) =

        let lessonState = lesson |> toLessonState user
        
        let st = lesson.StartDate |> SharedView.toCzTime
        let en = lesson.EndDate |> SharedView.toCzTime

        let avail =
            match lessonState with
            | Cancelled -> Tag.tag [ Tag.Color IsBlack ] [ str "Lekce zrušena" ]
            | AlreadyStarted(true) -> Tag.tag [ Tag.Color IsInfo ] [ str "Zůčastnili jste se" ]
            | AlreadyStarted(false) -> str ""
            | Reserved (Credits, _) -> Tag.tag [ Tag.Color IsInfo ] [ str "Zarezervováno" ]
            | Reserved (Cash, _) -> Tag.tag [ Tag.Color IsInfo ] [ str "Zarezervováno (hotově)" ]
            | NotReserved(Some Free, _, _) -> Tag.tag [ Tag.Color IsSuccess ] [ str "Volno" ]
            | NotReserved(Some LastFreeSpot, _, _) -> Tag.tag [ Tag.Color IsWarning ] [ str "Poslední volné místo" ]
            | NotReserved(None, _, _) -> Tag.tag [ Tag.Color IsDanger ] [ str "Lekce obsazena" ]

        let bookBtn =
            match user.IsAdmin, lessonState with
            | false, NotReserved(_,Some Cash,_) ->
                div [] [
                    Button.button [ Button.Color IsPrimary; Button.Option.Props [ OnClick (fun _ -> { LessonId = lesson.Id; UserReservation = ForOne(Payment.Cash) } |> AddReservation |> dispatch ) ] ]
                            [ str "Rezervovat (hotovost)" ]
                ]
            | false, NotReserved(_, Some Credits,_) ->
                div [] [
                    Button.button [ Button.Color IsPrimary; Button.Option.Props [ OnClick (fun _ -> { LessonId = lesson.Id; UserReservation = ForOne(Payment.Credits) } |> AddReservation |> dispatch ) ] ]
                        [ str "Rezervovat" ]
                ]
            | _ -> str ""

        let cancelBtn =
            match lessonState with
            | Reserved(_, true) ->
                div [] [
                    Button.button [ Button.Color IsDanger; Button.Props [ OnClick (fun _ -> CancelReservation(lesson.Id) |> dispatch) ] ] [
                        str "Zrušit rezervaci"
                    ]
                ]
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
                div [ ClassName "name"] [
                    str lesson.Name
                ]
                div [ ClassName "time"] [
                    i [ ClassName "far fa-clock"; Style [ MarginRight 5 ] ] []
                    sprintf "%s od %s do %s" (lesson.StartDate |> SharedView.toCzDate)  st en |> str
                ]
                div [] [
                    str lesson.Description
                ]
                div [ ClassName "buttons" ] [ bookBtn; cancelBtn ]
            ]
        ]

    let navigation (state:State) dispatch =
        
        let buttonBack =
            if state.WeekOffset <= maxLowerOffset then str ""
            else Button.button [ Button.Props [ OnClick (fun _ -> WeekOffsetChanged(state.WeekOffset - 1) |> dispatch) ] ] [
                    i [ ClassName "fas fa-chevron-circle-left" ] [ ]
                ]
        
        tr [ ClassName "controls" ] [
            td [ ColSpan 7 ] [
                buttonBack
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

    let col user (lessons:Lesson list) (workshops:Workshop list) (dispatch : Msg -> unit) (date:DateTimeOffset) =
        td [ ] [
            div [] (lessons |> List.map (lessonDiv user dispatch))
            div [] (workshops |> List.map workshopDiv)
        ]

    let render user (state : State) (dispatch : Msg -> unit) (startDate:DateTimeOffset, endDate:DateTimeOffset) =
        let dates = Yobo.Shared.DateRange.dateRangeToDays(startDate, endDate)
        let getLessonsForDate (date:DateTimeOffset) =
            state.Lessons
            |> List.filter (fun x -> x.StartDate.Date = date.Date)

        let getWorkshopsForDate (date:DateTimeOffset) =
            state.Workshops
            |> List.filter (fun x -> x.StartDate.Date = date.Date)

        let headerRow = dates |> List.map headerCol |> tr [ ClassName "header" ] 
        let row =
            dates
            |> List.map (fun x ->
                let lsns = x |> getLessonsForDate
                let workshps = x |> getWorkshopsForDate
                col user lsns workshps dispatch x
            )
            |> tr [ ClassName "day" ] 

        Table.table [ Table.CustomClass "is-fullwidth is-bordered table-calendar" ] [
            yield navigation state dispatch
            yield headerRow
            yield row
        ]

let render user (state : State) (dispatch : Msg -> unit) =
    div [] [
        Calendar.render user state dispatch (Yobo.Shared.DateRange.getDateRangeForWeekOffset state.WeekOffset)
    ]