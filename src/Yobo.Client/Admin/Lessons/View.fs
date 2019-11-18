module Yobo.Client.Admin.Lessons.View

open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
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
    open Yobo.Shared.Admin.Domain

    let col dispatch (lessons:Lesson list) (workshops:Workshop list) (date:DateTimeOffset) =
        let workshopDiv (workshop:Workshop) =
            let deleteBtn =
                if workshop.StartDate > DateTimeOffset.Now then
                    Button.button [ Button.Color IsDanger; Button.Props [ OnClick (fun _ -> DeleteWorkshop(workshop.Id) |> dispatch) ] ] [
                        str "Smazat workshop"
                    ]
                else "Workshop již proběhl" |> str |> SharedView.infoBox

            div [ ClassName "popover is-popover-bottom" ][
                div [ ClassName "popover-trigger lesson workshop" ] [
                    div [ ClassName "time" ] [
                        workshop.StartDate |> SharedView.toCzTime |> str
                        str " - "
                        workshop.EndDate |> SharedView.toCzTime |> str
                    ]
                    div [ ClassName "name"] [ str workshop.Name ]
                ]
                div [ ClassName "popover-content" ] [
                    deleteBtn
                ]
            ]

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
                        Button.button [ Button.Color IsWarning; Button.Props [ Style [ MarginLeft 5 ]; OnClick (fun _ -> CancelLesson(lesson.Id) |> dispatch) ] ] [
                            str "Zrušit lekci"
                        ]
                    else "Lekce již proběhla" |> str |> SharedView.infoBox

            let deleteBtn =
                if lesson.StartDate > DateTimeOffset.Now then
                    Button.button [ Button.Color IsDanger; Button.Props [ Style [ MarginLeft 5 ]; OnClick (fun _ -> DeleteLesson(lesson.Id) |> dispatch) ] ] [
                        str "Smazat lekci"
                    ]
                else str ""

            let editBtn =
                let lsn =
                    {
                        Id = lesson.Id
                        StartDate = lesson.StartDate |> SharedView.toCzDateTime
                        EndDate = lesson.EndDate |> SharedView.toCzDateTime
                        Name = lesson.Name
                        Description = lesson.Description
                    } : UpdateLessonForm
                if lesson.StartDate > DateTimeOffset.Now then
                    Button.button [ Button.Color IsInfo; Button.Props [ OnClick (fun _ -> UpdateLessonChanged (Some lsn) |> dispatch) ] ] [
                        str "Upravit lekci"
                    ]
                else str ""

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
                    editBtn
                    cancelBtn
                    deleteBtn
                ]
            ]


        td [ ] [
            div [] (lessons |> List.map lessonDiv)
            div [] (workshops |> List.map workshopDiv)
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
                Button.button [ Button.Color IsPrimary; Button.Props [ OnClick (fun _ -> AddLessonFormOpened(true) |> dispatch) ] ] [
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

    let lessonUpdateForm (lesson:UpdateLessonForm option) dispatch =
        match lesson with
        | None -> str ""
        | Some lsn ->
            tr [ ] [
                td [ ColSpan 7 ] [
                    Field.div [] [
                        Field.div [ Field.Option.IsHorizontal ] [
                            Field.label [ Field.Label.CustomClass "is-normal" ] [
                                Label.label [] [str "Začátek"]
                            ]
                            Field.body [] [
                                Field.div [ ] [
                                    div [ ClassName "control"] [
                                        Input.text [
                                            Input.Option.Placeholder "Čas začátku lekce, např. 19:00"
                                            Input.Option.Value (lsn.StartDate)
                                            Input.Option.OnChange (fun e ->
                                                !!e.target?value
                                                |> (fun v -> { lsn with StartDate = v })
                                                |> Some
                                                |> UpdateLessonChanged
                                                |> dispatch
                                            )
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
                                            Input.Option.Value (lsn.EndDate)
                                            Input.Option.OnChange (fun e ->
                                                !!e.target?value
                                                |> (fun v -> { lsn with EndDate = v })
                                                |> Some
                                                |> UpdateLessonChanged
                                                |> dispatch
                                            )
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
                                            Input.Option.Value lsn.Name
                                            Input.Option.OnChange (fun e ->
                                                !!e.target?value
                                                |> (fun v -> { lsn with Name = v })
                                                |> Some
                                                |> UpdateLessonChanged
                                                |> dispatch
                                            )
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
                                            Textarea.Option.Value lsn.Description
                                            Textarea.Option.OnChange (fun e ->
                                                !!e.target?value
                                                |> (fun v -> { lsn with Description = v })
                                                |> Some
                                                |> UpdateLessonChanged
                                                |> dispatch
                                            )
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
                                            Button.Color IsPrimary
                                            Button.Props [ OnClick (fun _ -> SubmitUpdateLessonForm |> dispatch) ]
                                        ] [ "Upravit" |> str ]

                                    ]
                                ]
                            ]
                        ]
                    ]
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
                                Input.Option.Value state.AddLessonForm.StartTime
                                Input.Option.OnChange (fun e -> !!e.target?value |> (fun v -> { state.AddLessonForm with StartTime = v }) |> AddLessonFormChanged |> dispatch)
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
                                Input.Option.Value state.AddLessonForm.EndTime
                                Input.Option.OnChange (fun e -> !!e.target?value |> (fun v -> { state.AddLessonForm with EndTime = v }) |> AddLessonFormChanged |> dispatch)
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
                                Input.Option.Value state.AddLessonForm.Name
                                Input.Option.OnChange (fun e -> !!e.target?value |> (fun v -> { state.AddLessonForm with Name = v }) |> AddLessonFormChanged |> dispatch)
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
                                Textarea.Option.Value state.AddLessonForm.Description
                                Textarea.Option.OnChange (fun e -> !!e.target?value |> (fun v -> { state.AddLessonForm with Description = v }) |> AddLessonFormChanged |> dispatch)
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
                                Button.Props [ OnClick (fun _ -> SubmitAddLessonForm |> dispatch) ]
                            ] [ sprintf "Přidat %i lekcí" (state.SelectedDates.Length) |> str ]
                            Button.button [
                                Button.Disabled (not isSubmitable)
                                Button.Color IsInfo
                                Button.Props [ Style [ MarginLeft 10]; OnClick (fun _ -> SubmitAddWorkshopForm |> dispatch) ]
                            ] [ sprintf "Přidat jako %i workshopů" (state.SelectedDates.Length) |> str ]
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
                        Delete.delete [ Delete.OnClick (fun _ -> AddLessonFormOpened(false) |> dispatch) ] [ ]
                    ]
                    Quickview.body [ ]
                        [ content ]
                    ]
            ]
        )

    let render (state : State) (dispatch : Msg -> unit) (startDate:DateTimeOffset, endDate:DateTimeOffset) =
        let dates = Yobo.Shared.DateRange.dateRangeToDays(startDate, endDate)
        let getLessonsForDate (date:DateTimeOffset) =
            state.Lessons
            |> List.filter (fun x -> x.StartDate.Date = date.Date)
        let getWorkshopsForDate (date:DateTimeOffset) =
            state.Workshops
            |> List.filter (fun x -> x.StartDate.Date = date.Date)

        let headerRow =
            dates
            |> List.map (headerCol state dispatch)
            |> tr [ ClassName "header" ]

        let row =
            dates
            |> List.map (fun x ->
                let lsns = x |> getLessonsForDate
                let wrksps = x |> getWorkshopsForDate
                col dispatch lsns wrksps x
            )
            |> tr [ ClassName "day" ]

        Table.table [ Table.CustomClass "is-fullwidth is-bordered table-calendar" ] [
            yield lessonUpdateForm state.UpdateLessonForm dispatch
            yield lessonsForm state dispatch
            yield navigation state dispatch
            yield headerRow
            yield row
        ]

let render (state : State) (dispatch : Msg -> unit) =
    div [] [
        Calendar.render state dispatch (Yobo.Shared.DateRange.getDateRangeForWeekOffset state.WeekOffset)
    ]