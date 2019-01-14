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

let private closestMonday (date:DateTime) =
    let offset = date.DayOfWeek - DayOfWeek.Monday
    date.AddDays -(offset |> float)

let private closestSunday (date:DateTime) =
    let current = date.DayOfWeek |> int
    let offset = 7 - current
    date.AddDays (offset |> float)

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

    let col (date:DateTime) =
        Column.column [ ] [ str "TADY BUDE OBSAH" ] // <| date.ToString("dd. MM.") ]

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

    let render (state : State) (dispatch : Msg -> unit) (startDate:DateTime) (endDate:DateTime) =
        let dates = datesBetween startDate endDate

        let headerRow = dates |> List.map headerCol |> Columns.columns [] 
        let row = dates |> List.map col |> Columns.columns [] 

        div [] [
            yield navigation state.Lessons.WeekOffset dispatch
            yield headerRow
            yield row
        ]


let render (state : State) (dispatch : Msg -> unit) =
    let s,e = getWeekDateRange (DateTime.UtcNow.AddDays(state.Lessons.WeekOffset * 7 |> float))
    div [] [
        Calendar.render state dispatch s e
    ]