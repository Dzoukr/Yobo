module Yobo.Client.Admin.Lessons.View

open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open System
open Yobo.Client.Admin.Lessons.Domain
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
            let cmd = if isChecked then DateUnselected else DateSelected 
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
            | _ -> ""
        Column.column [] [
            div [] [ str n ]
            div [] [ date.ToString("dd. MM. yyyy") |> str ]
        ]
        

    let navigation (state:State) dispatch =
        let addBtn =
            if state.SelectedDates.Length > 0 then
                Button.a [ Button.Color IsPrimary; Button.Props [ OnClick (fun _ -> FormOpened(true) |> dispatch) ] ] [
                    state.SelectedDates.Length |> sprintf "Přidat %i lekcí" |> str
                ]
            else str ""
        Columns.columns [] [
            Column.column [ ] [
                Button.a [ Button.Props [ OnClick (fun _ -> WeekOffsetChanged(state.WeekOffset - 1) |> dispatch) ] ] [
                    i [ ClassName "fas fa-chevron-circle-left" ] [ ]
                ]
                Button.a [ Button.Props [ OnClick (fun _ -> WeekOffsetChanged(0) |> dispatch) ] ] [
                    i [ ClassName "fas fa-home" ] [ ]
                ]
                Button.a [ Button.Props [ OnClick (fun _ -> WeekOffsetChanged(state.WeekOffset + 1) |> dispatch) ] ] [
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
            |> List.map (fun x -> x.ToString("dd. MM. yyyy"))
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
                            Button.a [ Button.Disabled (not isSubmitable); Button.Color IsPrimary; Button.Props [ (*OnClick (fun _ -> WeekOffsetChanged(offset - 1) |> LessonsMsg |> dispatch) ] ]*) ] ] [
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
        
    let render (state : State) (dispatch : Msg -> unit) (startDate:DateTime) (endDate:DateTime) =
        let dates = datesBetween startDate endDate

        let headerRow = dates |> List.map headerCol |> Columns.columns [] 
        let row = dates |> List.map (col state.SelectedDates dispatch) |> Columns.columns [] 

        div [] [
            yield lessonsForm state dispatch
            yield navigation state dispatch
            yield headerRow
            yield row
        ]



let render (state : State) (dispatch : Msg -> unit) =
    let s,e = getWeekDateRange (DateTime.UtcNow.AddDays(state.WeekOffset * 7 |> float))
    div [] [
        Calendar.render state dispatch s e
    ]