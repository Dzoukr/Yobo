module Yobo.Client.Admin.Lessons.View

open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open System
open Yobo.Client.Admin.Lessons.Domain
open Yobo.Shared
open Fulma.Extensions.Wikiki
open Yobo.Client
open Yobo.Shared.Extensions

module Calendar =
    open Yobo.Client
    open Yobo.Shared.Domain

    let col dispatch (lessons:Lesson list) (date:DateTimeOffset) =
        let lessonDiv (lesson:Lesson) =
            let cap =
                if lesson.IsCancelled then "Lekce je zrušena"
                else sprintf "%i / 12" lesson.Reservations.Length
            let res (u:User,r:UserReservation) =
                let _,useCredits = r.ToIntAndBool
                let cInfo = if not useCredits then "- (hotově)" else ""
                div [] [
                    i [ ClassName "fas fa-user"; Style [ MarginRight 5 ] ] []
                    sprintf "%s %s %s" u.FirstName u.LastName cInfo |> str
                ]

            let tagColor =
                match lesson.Reservations.Length with
                | 0 -> Tag.Color IsBlack
                | x when x > 0 && x < 12 -> Tag.Color IsWarning
                | _ -> Tag.Color IsSuccess

            let cancelBtn =
                if lesson.IsCancelled then
                    "Lekce je zrušena" |> str |> SharedView.warningBox
                else
                    if lesson.StartDate > DateTimeOffset.Now then
                        Button.button [ Button.Color IsDanger; Button.Props [ OnClick (fun _ -> CancelLesson(lesson.Id) |> dispatch) ] ] [
                            str "Zrušit lekci"
                        ]
                    else "Lekce již proběhla" |> str |> SharedView.infoBox

            let cancelledClass = if lesson.IsCancelled then "cancelled" else ""
            div [ ClassName "popover is-popover-bottom" ][
                div [ ClassName (sprintf "popover-trigger lesson %s" cancelledClass) ] [
                    div [ ClassName "time" ] [
                        lesson.StartDate |> SharedView.toCzTime |> str
                        str " - "
                        lesson.EndDate |> SharedView.toCzTime |> str
                        span [ ClassName "availability"; Style [ MarginLeft 5] ] [
                            Tag.tag [ tagColor ] [ str cap ]
                        ]
                    ]
                    div [ ClassName "name"] [ str lesson.Name ]
                    div [] (lesson.Reservations |> List.map res)
                ]
                div [ ClassName "popover-content" ] [
                    cancelBtn
                ]
            ]

            
        td [ ] [
            
            div [] (lessons |> List.map lessonDiv)
        ]

    let headerCol (state:State) (dispatch : Msg -> unit) (date:DateTimeOffset) =
        let isSelected = state.SelectedDates |> List.tryFind (fun y -> date = y) |> Option.isSome
        let checkBox =
            let cmd = if isSelected then DateUnselected else DateSelected 
            if date >= DateTimeOffset.Now.StartOfTheDay() then
                let i = date.Ticks.ToString()
                div [] [
                    Checkbox.input [
                        CustomClass "is-checkradio"
                        Props [ Props.Id i; Name i; Checked isSelected; OnChange (fun _ -> date |> cmd |> dispatch) ]
                    ]
                    Label.label [ Label.Option.For i ] [ str ""]
                ]
            else str ""
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
            div [] [ checkBox ]
            div [ ClassName "name" ] [ str n ]
            div [ ClassName "date" ] [ date.ToString("dd. MM. yyyy") |> str ]
        ]
        

    let navigation (state:State) dispatch =
        let addBtn =
            if state.SelectedDates.Length > 0 then
                Button.button [ Button.Color IsPrimary; Button.Props [ OnClick (fun _ -> FormOpened(true) |> dispatch) ] ] [
                    state.SelectedDates.Length |> sprintf "Přidat %i lekcí" |> str
                ]
            else str ""
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
                addBtn
            ]
        ]
        

    let lessonsForm (state:State) dispatch =
            
        let isSubmitable = Yobo.Client.Admin.Lessons.State.getValidLessonsToAdd state |> List.isEmpty |> not

        let dates =
            state.SelectedDates
            |> List.sort
            |> List.map SharedView.toCzDate
            |> String.concat ", "

        div [] [
            Field.div [ Field.Option.IsHorizontal ] [
                Field.label [ ] [ 
                    Label.label [] [str "Dny"]
                ]
                Field.body [] [
                    Field.div [ ] [
                        div [ ClassName "control"] [
                            str dates
                        ]
                    ]
                ]
            ]
            Field.div [ Field.Option.IsHorizontal ] [
                Field.label [ Field.Label.CustomClass "is-normal" ] [ 
                    Label.label [] [str "Začátek"]
                ]
                Field.body [] [
                    Field.div [ ] [
                        div [ ClassName "control"] [
                            Input.text [
                                Input.Option.Placeholder "Čas začátku lekce, např. 19:00"
                                Input.Option.Value state.StartTime
                                Input.Option.OnChange (fun e -> !!e.target?value |> StartChanged |> dispatch)
                            ]
                        ]
                    ]
                ]
            ]
            Field.div [ Field.Option.IsHorizontal ] [
                Field.label [ Field.Label.CustomClass "is-normal" ] [ 
                    Label.label [] [str "Konec"]
                ]
                Field.body [] [
                    Field.div [ ] [
                        div [ ClassName "control"] [
                            Input.text [
                                Input.Option.Placeholder "Čas konce lekce, např. 20:10"
                                Input.Option.Value state.EndTime
                                Input.Option.OnChange (fun e -> !!e.target?value |> EndChanged |> dispatch)
                            ]
                        ]
                    ]
                ]
            ]
            Field.div [ Field.Option.IsHorizontal ] [
                Field.label [ Field.Label.CustomClass "is-normal" ] [ 
                    Label.label [] [str "Název lekce"]
                ]
                Field.body [] [
                    Field.div [ ] [
                        div [ ClassName "control"] [
                            Input.text [
                                Input.Option.Value state.Name
                                Input.Option.OnChange (fun e -> !!e.target?value |> NameChanged |> dispatch)
                            ]
                        ]
                    ]
                ]
            ]
            Field.div [ Field.Option.IsHorizontal ] [
                Field.label [ Field.Label.CustomClass "is-normal" ] [ 
                    Label.label [] [str "Popis lekce"]
                ]
                Field.body [] [
                    Field.div [ ] [
                        div [ ClassName "control"] [
                            Textarea.textarea [
                                Textarea.Option.Value state.Description
                                Textarea.Option.OnChange (fun e -> !!e.target?value |> DescriptionChanged |> dispatch)
                            ] [ ]
                        ]
                    ]
                ]
            ]
            Field.div [ Field.Option.IsHorizontal ] [
                Field.label [ Field.Label.CustomClass "is-normal" ] [ 
                    Label.label [] []
                ]
                Field.body [] [
                    Field.div [ ] [
                        div [ ClassName "control"] [
                            Button.button [
                                Button.Disabled (not isSubmitable)
                                Button.Color IsPrimary
                                Button.Props [ OnClick (fun _ -> SubmitLessonsForm |> dispatch) ]
                            ] [
                                sprintf "Přidat %i lekcí" (state.SelectedDates.Length) |> str
                            ]
                        ]
                    ]
                ]
            ]
        ]
        |> (fun content ->
            div [ ] [
                Quickview.quickview [ Quickview.IsActive state.FormOpened ] [
                    Quickview.header [ ] [
                        Quickview.title [ ] [ str "Přidat lekce" ]
                        Delete.delete [ Delete.OnClick (fun _ -> FormOpened(false) |> dispatch) ] [ ]
                    ]
                    Quickview.body [ ]
                        [ content ]
                    ]
            ]
        )

    let render (state : State) (dispatch : Msg -> unit) (startDate:DateTimeOffset, endDate:DateTimeOffset) =
        let dates = DateRange.dateRangeToDays(startDate, endDate)
        let getLessonsForDate (date:DateTimeOffset) =
            state.Lessons
            |> List.filter (fun x -> x.StartDate.Date = date.Date)

        let headerRow =
            dates
            |> List.map (headerCol state dispatch)
            |> tr [ ClassName "header" ]

        let row =
            dates
            |> List.map (fun x ->
                let lsns = x |> getLessonsForDate
                col dispatch lsns x
            )
            |> tr [ ClassName "day" ]

        Table.table [ Table.CustomClass "is-fullwidth is-bordered table-calendar" ] [
            yield lessonsForm state dispatch
            yield navigation state dispatch
            yield headerRow
            yield row
        ]

let render (state : State) (dispatch : Msg -> unit) =
    div [] [
        Calendar.render state dispatch (DateRange.getDateRangeForWeekOffset state.WeekOffset)
    ]