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

let toDate (h,m) = DateTime.Today.AddHours(float h).AddMinutes(float m)

let formQuickView model dispatch =
    
    let lessonsForm (f:ValidatedForm<Request.CreateLessons>) =
        
        Html.div [
            Bulma.field [
                Bulma.label "Termíny"
                Bulma.fieldBody [
                    f.FormData.Dates
                    |> List.map DateTimeOffset.toCzDate
                    |> String.concat ", "
                    |> Html.span
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Dates))
            ]
            
            Bulma.field [
                Bulma.label "Začátek a konec"
                Bulma.fieldBody [
                    Calendar.calendar [
                        prop.id "expCal"
                        calendar.options [
                            calendar.options.experimental.triggerOnTimeChange true
                            calendar.options.experimental.liveUpdate true
                            calendar.options.type'.time
                            calendar.options.isRange true
                            calendar.options.minuteSteps 1
                            calendar.options.displayMode.inline'
                            calendar.options.showFooter false
                            calendar.options.startDate (f.FormData.StartTime |> toDate)
                            calendar.options.endDate (f.FormData.EndTime |> toDate)
                        ]
                        calendar.onValueSelected (fun x ->
                            match x with
                            | RangeValue (RangeValue.Time (s,e)) ->
                                { f.FormData with StartTime = s.Hours,s.Minutes; EndTime = e.Hours,e.Minutes } |> LessonsFormChanged |> dispatch
                            | _ -> ()
                        )
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.StartTime))
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.EndTime))
            ]
            Bulma.field [
                Bulma.label "Název"
                Bulma.fieldBody [
                    Bulma.textInput [
                        ValidationViews.color f.ValidationErrors (nameof(f.FormData.Name))
                        prop.onTextChange (fun x -> { f.FormData with Name = x } |> LessonsFormChanged |> dispatch)
                        prop.valueOrDefault (f.FormData.Name)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Name))
            ]
            Bulma.field [
                Bulma.label "Popis"
                Bulma.fieldBody [
                    Bulma.textarea [
                        ValidationViews.color f.ValidationErrors (nameof(f.FormData.Description))
                        prop.onTextChange (fun x -> { f.FormData with Description = x } |> LessonsFormChanged |> dispatch)
                        prop.valueOrDefault (f.FormData.Description)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Description))
            ]
            Bulma.field [
                Bulma.label "Kapacita"
                Bulma.fieldBody [
                    Bulma.numberInput [
                        ValidationViews.color f.ValidationErrors (nameof(f.FormData.Capacity))
                        prop.onTextChange (fun x -> { f.FormData with Capacity = int x } |> LessonsFormChanged |> dispatch)
                        prop.valueOrDefault (f.FormData.Capacity)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Capacity))
            ]
            Bulma.field [
                Bulma.fieldBody [
                    Bulma.button [
                        button.isPrimary
                        prop.text "Přidat lekce"
                        if f.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                        prop.onClick (fun _ -> CreateLessons |> dispatch)
                    ]
                ]
            ]
        ]
    
    let workshopsForm (f:ValidatedForm<Request.CreateWorkshops>) =
        Html.div [
            Bulma.field [
                Bulma.label "Termíny"
                Bulma.fieldBody [
                    f.FormData.Dates
                    |> List.map DateTimeOffset.toCzDate
                    |> String.concat ", "
                    |> Html.span
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Dates))
            ]
            
            Bulma.field [
                Bulma.label "Začátek a konec"
                Bulma.fieldBody [
                    Calendar.calendar [
                        prop.id "workCal"
                        calendar.options [
                            calendar.options.experimental.triggerOnTimeChange true
                            calendar.options.type'.time
                            calendar.options.isRange true
                            calendar.options.minuteSteps 1
                            calendar.options.displayMode.inline'
                            calendar.options.showFooter false
                            calendar.options.startDate (f.FormData.StartTime |> toDate)
                            calendar.options.endDate (f.FormData.EndTime |> toDate)
                        ]
                        calendar.onValueSelected (fun x ->
                            match x with
                            | RangeValue (RangeValue.Time (s,e)) ->
                                { f.FormData with StartTime = s.Hours,s.Minutes; EndTime = e.Hours,e.Minutes } |> WorkshopsFormChanged |> dispatch
                            | _ -> ()
                        )
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.StartTime))
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.EndTime))
            ]
            Bulma.field [
                Bulma.label "Název"
                Bulma.fieldBody [
                    Bulma.textInput [
                        ValidationViews.color f.ValidationErrors (nameof(f.FormData.Name))
                        prop.onTextChange (fun x -> { f.FormData with Name = x } |> WorkshopsFormChanged |> dispatch)
                        prop.valueOrDefault (f.FormData.Name)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Name))
            ]
            Bulma.field [
                Bulma.label "Popis"
                Bulma.fieldBody [
                    Bulma.textarea [
                        ValidationViews.color f.ValidationErrors (nameof(f.FormData.Description))
                        prop.onTextChange (fun x -> { f.FormData with Description = x } |> WorkshopsFormChanged |> dispatch)
                        prop.valueOrDefault (f.FormData.Description)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Description))
            ]
            
            Bulma.field [
                Bulma.fieldBody [
                    Bulma.button [
                        button.isPrimary
                        prop.text "Přidat workshopy"
                        if f.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                        prop.onClick (fun _ -> CreateWorkshops |> dispatch)
                    ]
                ]
            ]
        ]
        
    let onlinesForm (f:ValidatedForm<Request.CreateOnlineLessons>) =
        
        Html.div [
            Bulma.field [
                Bulma.label "Termíny"
                Bulma.fieldBody [
                    f.FormData.Dates
                    |> List.map DateTimeOffset.toCzDate
                    |> String.concat ", "
                    |> Html.span
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Dates))
            ]
            
            Bulma.field [
                Bulma.label "Začátek a konec"
                Bulma.fieldBody [
                    Calendar.calendar [
                        prop.id "expCal"
                        calendar.options [
                            calendar.options.experimental.triggerOnTimeChange true
                            calendar.options.experimental.liveUpdate true
                            calendar.options.type'.time
                            calendar.options.isRange true
                            calendar.options.minuteSteps 1
                            calendar.options.displayMode.inline'
                            calendar.options.showFooter false
                            calendar.options.startDate (f.FormData.StartTime |> toDate)
                            calendar.options.endDate (f.FormData.EndTime |> toDate)
                        ]
                        calendar.onValueSelected (fun x ->
                            match x with
                            | RangeValue (RangeValue.Time (s,e)) ->
                                { f.FormData with StartTime = s.Hours,s.Minutes; EndTime = e.Hours,e.Minutes } |> OnlineLessonsFormChanged |> dispatch
                            | _ -> ()
                        )
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.StartTime))
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.EndTime))
            ]
            Bulma.field [
                Bulma.label "Název"
                Bulma.fieldBody [
                    Bulma.textInput [
                        ValidationViews.color f.ValidationErrors (nameof(f.FormData.Name))
                        prop.onTextChange (fun x -> { f.FormData with Name = x } |> OnlineLessonsFormChanged |> dispatch)
                        prop.valueOrDefault (f.FormData.Name)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Name))
            ]
            Bulma.field [
                Bulma.label "Popis"
                Bulma.fieldBody [
                    Bulma.textarea [
                        ValidationViews.color f.ValidationErrors (nameof(f.FormData.Description))
                        prop.onTextChange (fun x -> { f.FormData with Description = x } |> OnlineLessonsFormChanged |> dispatch)
                        prop.valueOrDefault (f.FormData.Description)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Description))
            ]
            Bulma.field [
                Bulma.label "Kapacita"
                Bulma.fieldBody [
                    Bulma.numberInput [
                        ValidationViews.color f.ValidationErrors (nameof(f.FormData.Capacity))
                        prop.onTextChange (fun x -> { f.FormData with Capacity = int x } |> OnlineLessonsFormChanged |> dispatch)
                        prop.valueOrDefault (f.FormData.Capacity)
                    ]
                ]
                ValidationViews.help f.ValidationErrors (nameof(f.FormData.Capacity))
            ]
            Bulma.field [
                Bulma.fieldBody [
                    Bulma.button [
                        button.isPrimary
                        prop.text "Přidat online lekce"
                        if f.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                        prop.onClick (fun _ -> CreateOnlineLessons |> dispatch)
                    ]
                ]
            ]
        ]

    
    let activeForm =
        match model.ActiveForm with
        | LessonsForm -> model.LessonsForm |> lessonsForm
        | WorkshopsForm -> model.WorkshopsForm |> workshopsForm
        | OnlinesForm -> model.OnlinesForm |> onlinesForm
    
    let form = [
        let item (name:string) formType =
            Html.li [                       
                if model.ActiveForm = formType then tabs.isActive
                prop.children [
                    Html.a [
                        Html.span name
                    ]
                ]
                prop.onClick (fun _ -> formType |> SwitchActiveForm |> dispatch)
            ]
            
        Bulma.tabs [
            tabs.isCentered
            tabs.isToggle
            prop.children [
                Html.ul [
                    item "Lekce" ActiveForm.LessonsForm
                    item "Workshop" WorkshopsForm
                    item "Online lekce" OnlinesForm
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