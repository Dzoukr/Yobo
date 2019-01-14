module Yobo.Client.Admin.Lessons.View

open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open System
open Yobo.Client.Admin.Domain
open Yobo.Shared
open Yobo.Shared.Text
open Yobo.Shared
open Fulma.Extensions.Wikiki
open FSharp.Rop

let private closestMonday (date:DateTime) =
    let offset = date.DayOfWeek - DayOfWeek.Monday
    date.AddDays -(offset |> float) |> fun x -> x.Date

let private closestSunday (date:DateTime) =
    let current = date.DayOfWeek |> int
    let offset = 7 - current
    date.AddDays (offset |> float) |> fun x -> x.Date

let private getWeekDateRange dayInWeek =
    (dayInWeek |> closestMonday), (dayInWeek |> closestSunday)
    
let private datesBetween (startDate:DateTime) (endDate:DateTime) =
    endDate.Subtract(startDate).TotalDays
    |> int
    |> (fun d -> [0..d])
    |> List.map (fun x ->
        startDate.AddDays (float x)
    )

module Calendar =
    open Yobo.Client

    //let test =
    //    div [ ClassName "popover is-popover-bottom"] [
    //        Button.button [ Button.Option.CustomClass "is-primary popover-trigger"] [
    //            str "Jemna joga"
    //        ]
    //        div [ ClassName "popover-content" ] [
    //            Table.table [] [
    //                thead [] [
    //                    tr [][
    //                        th [] [ str "Jemna joga s relaxaci"]
    //                    ]
    //                ]
    //                tbody [] [
    //                    tr [] [
    //                        td [] [
    //                            h1 [] [str "Klidná lekce s nenáročným průběhem s důrazem na dech a správnost provádění ve vazbě na zdravotní aspekt jógy. Jóga vhodná i pro seniory a pro všechny se sníženou pohyblivostí, těhotné a po porodu."]
    //                        ]
    //                    ]
    //                ]
    //            ]
    //        ]
    //    ]

    let col (selectedDates:DateTime list) (dispatch : Msg -> unit) (date:DateTime) =
        let checkBox =
            let isChecked = selectedDates |> List.tryFind (fun x -> x = date) |> Option.isSome
            let cmd = if isChecked then LessonsMsg.DateUnselected >> LessonsMsg else LessonsMsg.DateSelected >> LessonsMsg
            if date >= DateTime.UtcNow then
                let i = date.Ticks.ToString()
                div [] [
                    Checkbox.input [
                        CustomClass "is-checkradio"
                        Props [ Props.Id i; Name i; Checked isChecked; OnChange (fun _ -> date |> cmd |> dispatch) ]
                    ]
                    Label.label [ Label.Option.For i ] [ str ""]
                ]
            else str ""

        Column.column [ ] [
            checkBox
        ]

    let headerCol (date:DateTime) =
        let n =
            match date.DayOfWeek with
            | DayOfWeek.Monday -> "Pondělí"
            | DayOfWeek.Tuesday -> "Úterý"
            | DayOfWeek.Wednesday -> "Středa"
            | DayOfWeek.Thursday -> "Čtvrtek"
            | DayOfWeek.Friday -> "Pátek"
            | DayOfWeek.Saturday -> "Sobota"
            | DayOfWeek.Sunday -> "Neděle"
        Column.column [] [
            div [] [ str n ]
            div [] [ date.ToString("dd. MM. yyyy") |> str ]
        ]
        

    let navigation offset dispatch =
        Columns.columns [] [
            Column.column [ ] [
                Button.a [ Button.Props [ OnClick (fun _ -> WeekOffsetChanged(offset - 1) |> LessonsMsg |> dispatch) ] ] [
                    i [ ClassName "fas fa-chevron-circle-left" ] [ ]
                ]
                Button.a [ Button.Props [ OnClick (fun _ -> WeekOffsetChanged(0) |> LessonsMsg |> dispatch) ] ] [
                    i [ ClassName "fas fa-home" ] [ ]
                ]
                Button.a [ Button.Props [ OnClick (fun _ -> WeekOffsetChanged(offset + 1) |> LessonsMsg |> dispatch) ] ] [
                    i [ ClassName "fas fa-chevron-circle-right" ] [ ]
                ]
            ]
        ]

    let tryToTimeSpan (s:string) =
        let parts = s.Split([|':'|]) |> Array.toList
        match parts with
        | [h;m] ->
            match Int32.TryParse h, Int32.TryParse m with
            | (true, h), (true, m) ->
                TimeSpan(h, m, 0) |> Some
            | _ -> None
        | _ -> None

    let tryGetFromTo (s:string) (e:string) (d:DateTime) =
        match tryToTimeSpan s, tryToTimeSpan e with
        | Some st, Some en -> (d.Date.Add(st), d.Date.Add(en)) |> Some
        | _ -> None

    let getValidLessonsToAdd (state:LessonsState) =
        state.SelectedDates
        |> List.map (fun x ->
            let st,en =
                x
                |> tryGetFromTo state.StartTime state.EndTime
                |> Option.defaultValue (DateTime.MinValue, DateTime.MinValue)
            ({
                Start = st
                End = en
                Name = state.Name
                Description = state.Description
            } : Yobo.Shared.Admin.Domain.AddLesson)
        )
        |> List.filter (fun x -> x.Start <> DateTime.MinValue)
        |> List.map Yobo.Shared.Admin.Validation.validateAddLesson
        |> Result.partition
        |> fst
        

    let lessonsForm (state:LessonsState) dispatch =
        if state.SelectedDates.Length > 0 then
            
            let isSubmitable = getValidLessonsToAdd state |> List.isEmpty |> not

            let dates =
                state.SelectedDates
                |> List.sort
                |> List.map (fun x -> x.ToString("dd. MM. yyyy"))
                |> String.concat ", "
            Columns.columns [] [
                Column.column [] [
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
                                        Input.Option.OnChange (fun e -> !!e.target?value |> StartChanged |> LessonsMsg |> dispatch)
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
                                        Input.Option.OnChange (fun e -> !!e.target?value |> EndChanged |> LessonsMsg |> dispatch)
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
                                        Input.Option.OnChange (fun e -> !!e.target?value |> NameChanged |> LessonsMsg |> dispatch)
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
                                        Textarea.Option.OnChange (fun e -> !!e.target?value |> DescriptionChanged |> LessonsMsg |> dispatch)
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
                                    Button.a [ Button.Disabled (not isSubmitable); Button.Color IsPrimary; Button.Props [ (*OnClick (fun _ -> WeekOffsetChanged(offset - 1) |> LessonsMsg |> dispatch) ] ]*) ] ] [
                                        sprintf "Přidat %i lekcí" (state.SelectedDates.Length) |> str
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        else str ""
        
    let render (state : State) (dispatch : Msg -> unit) (startDate:DateTime) (endDate:DateTime) =
        let dates = datesBetween startDate endDate

        let headerRow = dates |> List.map headerCol |> Columns.columns [] 
        let row = dates |> List.map (col state.Lessons.SelectedDates dispatch) |> Columns.columns [] 

        div [] [
            yield lessonsForm state.Lessons dispatch
            yield navigation state.Lessons.WeekOffset dispatch
            yield headerRow
            yield row
        ]



let render (state : State) (dispatch : Msg -> unit) =
    let s,e = getWeekDateRange (DateTime.UtcNow.AddDays(state.Lessons.WeekOffset * 7 |> float))
    div [] [
        Calendar.render state dispatch s e
    ]