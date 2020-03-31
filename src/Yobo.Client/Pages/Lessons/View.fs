module Yobo.Client.Pages.Lessons.View

open System
open Domain
open Feliz
open Feliz.Bulma
open Feliz.Bulma.Checkradio
open Feliz.Bulma.QuickView
open Feliz.Bulma.Calendar
open Feliz.Bulma.Operators
open Yobo.Client.Forms
open Yobo.Shared.DateTime
open Yobo.Client.SharedView
open Yobo.Shared.Core.Admin.Communication

let headerTd (state:Model) (dispatch : Msg -> unit) (date:DateTimeOffset) =
    let checkBox =
        if date >= (DateTimeOffset.Now |> DateTimeOffset.startOfTheDay) then
            let isSelected = state.SelectedDates |> List.tryFind (fun y -> date = y) |> Option.isSome
            let i = date.Ticks.ToString()
            Html.div [
                Checkradio.checkbox [
                    prop.id i
                    prop.isChecked isSelected
                    color.isSuccess
                    prop.onCheckedChange (fun _ -> date |> ToggleDate |> dispatch )
                ]
                Html.label [
                    prop.htmlFor i
                ]
            ]
        else Html.none
    Html.td [
        Html.div [ checkBox ]
        Html.div [ prop.className "name"; prop.text (date.DayOfWeek |> DayOfWeek.toCzDay) ]
        Html.div [ prop.className "date"; prop.text (date |> DateTimeOffset.toCzDate) ]
    ]

let headerRow model dispatch dates =
    dates
    |> List.map (headerTd model dispatch)
    |> (fun x -> Html.tr [ prop.className "header"; prop.children x ])
    
let navigationRow (model:Model) dispatch =
    let addBtn =
        if model.SelectedDates.Length > 0 then
            Bulma.button [
                button.isPrimary
                prop.onClick (fun _ -> ShowForm true |> dispatch)
                prop.text (model.SelectedDates.Length |> sprintf "Přidat %i události") 
            ]
        else Html.none
    
    Html.tr [
        prop.className "controls"
        prop.children [
            Html.td [
                prop.colSpan 7
                prop.children [
                    Bulma.button [
                        prop.onClick (fun _ -> WeekOffsetChanged(model.WeekOffset - 1) |> dispatch)
                        prop.children [ Html.faIconSingle "fas fa-chevron-circle-left" ]
                    ]
                    Bulma.button [
                        prop.onClick (fun _ -> WeekOffsetChanged(0) |> dispatch)
                        prop.children [ Html.faIconSingle "fas fa-home" ]
                    ]
                    Bulma.button [
                        prop.onClick (fun _ -> WeekOffsetChanged(model.WeekOffset + 1) |> dispatch)
                        prop.children [ Html.faIconSingle "fas fa-chevron-circle-right" ]
                    ]
                    addBtn
                ]
            ]
        ]
    ]

let formQuickView model dispatch =
    
    let toTime (s:string) =
        let parts = s.Split[|':'|]
        (int parts.[0]), (int parts.[1])
    
    let toValue (hrs,mn) = sprintf "%i:%i" hrs mn        
    
    let lessonsForm (f:ValidatedForm<Request.CreateLessons>) =
        Html.div [
            Bulma.field [
                Bulma.label "Termíny"
                Bulma.fieldBody [
                    model.SelectedDates
                    |> List.map DateTimeOffset.toCzDate
                    |> String.concat ", "
                    |> Html.span
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Dates))
            ]
            Bulma.field [
                Bulma.label "Začátek"
                Bulma.fieldBody [
                    Bulma.textInput [
                        ValidationViews.color model.LessonsForm.ValidationErrors (nameof(model.LessonsForm.FormData.StartTime))
                        prop.onTextChange (fun x -> { model.LessonsForm.FormData with StartTime = toTime x } |> LessonsFormChanged |> dispatch)
                        prop.valueOrDefault (model.LessonsForm.FormData.StartTime |> toValue)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.StartTime))
            ]
            Bulma.field [
                Bulma.label "Konec"
                Bulma.fieldBody [
                    Bulma.textInput [
                        ValidationViews.color model.LessonsForm.ValidationErrors (nameof(model.LessonsForm.FormData.EndTime))
                        prop.onTextChange (fun x -> { model.LessonsForm.FormData with EndTime = toTime x } |> LessonsFormChanged |> dispatch)
                        prop.valueOrDefault (model.LessonsForm.FormData.EndTime |> toValue)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.EndTime))
            ]
            Bulma.field [
                Bulma.label "Název"
                Bulma.fieldBody [
                    Bulma.textInput [
                        ValidationViews.color model.LessonsForm.ValidationErrors (nameof(model.LessonsForm.FormData.Name))
                        prop.onTextChange (fun x -> { model.LessonsForm.FormData with Name = x } |> LessonsFormChanged |> dispatch)
                        prop.valueOrDefault (model.LessonsForm.FormData.Name)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Name))
            ]
            Bulma.field [
                Bulma.label "Popis"
                Bulma.fieldBody [
                    Bulma.textarea [
                        ValidationViews.color model.LessonsForm.ValidationErrors (nameof(model.LessonsForm.FormData.Description))
                        prop.onTextChange (fun x -> { model.LessonsForm.FormData with Description = x } |> LessonsFormChanged |> dispatch)
                        prop.valueOrDefault (model.LessonsForm.FormData.Description)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Description))
            ]
            Bulma.field [
                Bulma.label "Kapacita"
                Bulma.fieldBody [
                    Bulma.numberInput [
                        ValidationViews.color model.LessonsForm.ValidationErrors (nameof(model.LessonsForm.FormData.Capacity))
                        prop.onTextChange (fun x -> { model.LessonsForm.FormData with Capacity = int x } |> LessonsFormChanged |> dispatch)
                        prop.valueOrDefault (model.LessonsForm.FormData.Capacity)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Capacity))
            ]
            Bulma.field [
                Bulma.fieldBody [
                    Bulma.button [
                        button.isPrimary
                        prop.text "Přidat lekce"
                        if model.LessonsForm.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                        prop.onClick (fun _ -> CreateLessons |> dispatch)
                    ]
                ]
            ]
            
            
        ]
        
    
    let activeForm =
        match model.ActiveForm with
        | LessonsForm -> model.LessonsForm |> lessonsForm
    
    let form = [
        Bulma.tabs [
            tabs.isCentered
            tabs.isToggle
            prop.children [
                Html.ul [
                    Html.li [
                        tabs.isActive
                        prop.children [
                            Html.a [
                                Html.span "Lekce"
                            ]
                        ]
                    ]
                    Html.li [
                        Html.a [
                            Html.span "Workshop"
                        ]
                    ]
                    Html.li [
                        Html.a [
                            Html.span "Online lekce"
                        ]
                    ]
                ]
            ]
        ]
        activeForm
    ]
    
    QuickView.quickview [
        quickview.isActive
        prop.children [
            QuickView.header [
                Html.div ""
                Bulma.delete [ prop.onClick (fun _ -> false |> ShowForm |> dispatch) ]
            ]
            QuickView.body [
                QuickView.block [
                    prop.style [ style.padding 15 ]
                    prop.children form
                ]
            ]
        ]
    ]

let view (model:Model) (dispatch: Msg -> unit) =
    
    let dates =
        model.WeekOffset
        |> DateRange.getDateRangeForWeekOffset
        |> DateRange.dateRangeToDays
    
    Html.div [
        if model.FormShown then formQuickView model dispatch
        Bulma.table [
            table.isFullwidth
            table.isBordered
            ++ prop.className "table-calendar"
            prop.children [
                navigationRow model dispatch
                dates |> headerRow model dispatch
            ]
        ]
    ]