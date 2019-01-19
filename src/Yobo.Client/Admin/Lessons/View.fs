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




module Calendar =
    open Yobo.Client
    open Yobo.Shared.Admin.Domain

    

    let col isSelected (lessons:Lesson list) (dispatch : Msg -> unit) (date:DateTimeOffset) =
        let checkBox =
            let cmd = if isSelected then DateUnselected else DateSelected 
            if date >= DateTimeOffset.Now then
                let i = date.Ticks.ToString()
                div [] [
                    Checkbox.input [
                        CustomClass "is-checkradio"
                        Props [ Props.Id i; Name i; Checked isSelected; OnChange (fun _ -> date |> cmd |> dispatch) ]
                    ]
                    Label.label [ Label.Option.For i ] [ str ""]
                ]
            else str ""

        let lessonDiv (lesson:Lesson) =
            let st = lesson.StartDate |> SharedView.toCzTime
            let en = lesson.EndDate |> SharedView.toCzTime
            let cap = sprintf "Přihlášeno %i z 12" lesson.Reservations.Length
            let res (u:User,c:int) =
                div [] [
                    sprintf "%s %s - %ix" u.FirstName u.LastName c |> str
                ]

            div [] [
                div [] [ sprintf "%s - %s" st en |> str ]
                div [] [ cap |> str ]
                div [] (lesson.Reservations |> List.map res)
                hr []
            ]

        Column.column [ ] [
            checkBox
            div [] (lessons |> List.map lessonDiv)
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
            div [] [ date |> SharedView.toCzDate |> str ]
        ]
        

    let navigation (state:State) dispatch =
        let addBtn =
            if state.SelectedDates.Length > 0 then
                Button.button [ Button.Color IsPrimary; Button.Props [ OnClick (fun _ -> FormOpened(true) |> dispatch) ] ] [
                    state.SelectedDates.Length |> sprintf "Přidat %i lekcí" |> str
                ]
            else str ""
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

        let headerRow = dates |> List.map headerCol |> Columns.columns [] 
        let row =
            dates
            |> List.map (fun x ->
                let lsns = x |> getLessonsForDate
                let isSelected = state.SelectedDates |> List.tryFind (fun y -> x = y) |> Option.isSome
                col isSelected lsns dispatch x

            )
            |> Columns.columns [] 

        div [] [
            yield lessonsForm state dispatch
            yield navigation state dispatch
            yield headerRow
            yield row
        ]

let render (state : State) (dispatch : Msg -> unit) =
    div [] [
        Calendar.render state dispatch (DateRange.getDateRangeForWeekOffset state.WeekOffset)
    ]