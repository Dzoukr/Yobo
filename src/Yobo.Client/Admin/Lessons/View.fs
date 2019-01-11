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

let private getDateRange year month =
    let startOfMonth = DateTime(year, month, 1)
    let endOfMonth = DateTime(year, month, DateTime.DaysInMonth(year, month))
    (startOfMonth |> closestMonday), (endOfMonth |> closestSunday)

let private datesBetween (startDate:DateTime) (endDate:DateTime) =
    endDate.Subtract(startDate).TotalDays
    |> int
    |> (fun d -> [0..d])
    |> List.map (fun x ->
        startDate.AddDays (float x)
    )

let private chunkBySize size list =
    let foldFn acc item =
        match acc with
        | 0, li -> 1, [item] :: li
        | x, li ->
            let ni = if x = (size - 1) then 0 else x + 1
            match li with
            | h :: t -> ni, ((item :: h) :: t)
            | [] -> ni, [[item]]
    
    list |> List.fold foldFn (0, []) |> snd |> List.rev |> List.map List.rev


module Calendar =

    let col (date:DateTime) =
        Column.column [ ] [ str <| date.ToString("dd. MM.") ]

    let render (startDate:DateTime) (endDate:DateTime) =
        let dates = datesBetween startDate endDate
        let header =
            [ "Pondělí";"Úterý";"Středa";"Čtvrtek";"Pátek";"Sobota";"Neděle"]
            |> List.map (fun x ->
                Column.column [ ] [
                    strong [] [ str x ]
                ]
            )
        
        let row (dates:DateTime list) =
            dates
            |> List.map col
            |> Columns.columns [] 

        let rows =
            dates
            |> chunkBySize 7
            |> List.map row

        div [] [
            yield Columns.columns [] header
            yield! rows
        ]


let render (state : State) (dispatch : Msg -> unit) =
    
    let s,e = getDateRange state.Lessons.SelectedYear state.Lessons.SelectedMonth
    //dispatch
    div [] [
        Calendar.render s e
    ]