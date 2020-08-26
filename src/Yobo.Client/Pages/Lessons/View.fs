module Yobo.Client.Pages.Lessons.View

open System
open Domain
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Feliz.Bulma.Checkradio
open Feliz.Bulma.QuickView
open Feliz.Bulma.Calendar
open Feliz.Bulma.Operators
open Yobo.Client.Forms
open Yobo.Shared.DateTime
open Yobo.Client.SharedView
open Yobo.Shared.Core
open Yobo.Shared.Core.Admin.Communication
open Yobo.Shared.Core.Admin.Domain
open Yobo.Shared.Core.Admin.Domain.Queries

let headerTd (state:Model) (dispatch : Msg -> unit) (date:DateTimeOffset) =
    let checkBox =
        Html.div [
            if date >= (DateTimeOffset.Now |> DateTimeOffset.startOfTheDay) then
                let isSelected = state.SelectedDates |> List.tryFind (fun y -> date = y) |> Option.isSome
                let i = date.Ticks.ToString()
                Bulma.field.div [
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
        ]
        
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
            Bulma.dropdown [
                dropdown.isHoverable
                prop.children [
                    Bulma.dropdownTrigger [
                        Bulma.button.button [
                            color.isPrimary
                            prop.children [
                                Html.span (model.SelectedDates.Length |> sprintf "Přidat %i události")
                                Bulma.icon [
                                    icon.isSmall
                                    prop.children [ Html.faIconSingle "fas fa-angle-down" ] 
                                ]
                            ]
                        ]
                    ]
                    Bulma.dropdownMenu [
                        Bulma.dropdownContent [
                            Bulma.dropdownItem.a [
                                prop.text "Lekce"; prop.onClick (fun _ -> SelectActiveForm (Some LessonsForm) |> dispatch)
                            ]
                            Bulma.dropdownItem.a [
                                prop.text "Workshopy"; prop.onClick (fun _ -> SelectActiveForm (Some WorkshopsForm) |> dispatch)
                            ]
                        ]
                    ]
                ]
            ]
           
        else Html.none
    
    Html.tr [
        prop.className "controls"
        prop.children [
            Html.td [
                prop.colSpan 7
                prop.children [
                    Bulma.button.button [
                        prop.onClick (fun _ -> WeekOffsetChanged(model.WeekOffset - 1) |> dispatch)
                        prop.children [ Html.faIconSingle "fas fa-chevron-circle-left" ]
                    ]
                    Bulma.button.button [
                        prop.onClick (fun _ -> WeekOffsetChanged(0) |> dispatch)
                        prop.children [ Html.faIconSingle "fas fa-home" ]
                    ]
                    Bulma.button.button [
                        prop.onClick (fun _ -> WeekOffsetChanged(model.WeekOffset + 1) |> dispatch)
                        prop.children [ Html.faIconSingle "fas fa-chevron-circle-right" ]
                    ]
                    addBtn
                ]
            ]
        ]
    ]

let toDate (h,m) = DateTime.Today.AddHours(float h).AddMinutes(float m)
    
let lessonsForm (f:ValidatedForm<Request.CreateLessons>) dispatch =
    
    Html.div [
        Bulma.field.div [
            Bulma.label "Termíny"
            Bulma.fieldBody [
                f.FormData.Dates
                |> List.map DateTimeOffset.toCzDate
                |> String.concat ", "
                |> Html.span
            ]
            ValidationViews.help f.ValidationErrors (nameof(f.FormData.Dates))
        ]
        
        Bulma.field.div [
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
        Bulma.field.div [
            Bulma.label "Název"
            Bulma.fieldBody [
                Bulma.input.text [
                    ValidationViews.color f.ValidationErrors (nameof(f.FormData.Name))
                    prop.onTextChange (fun x -> { f.FormData with Name = x } |> LessonsFormChanged |> dispatch)
                    prop.valueOrDefault (f.FormData.Name)
                ]
            ]
            ValidationViews.help f.ValidationErrors (nameof(f.FormData.Name))
        ]
        Bulma.field.div [
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
        Bulma.field.div [
            Bulma.label "Kapacita"
            Bulma.fieldBody [
                Bulma.input.number [
                    ValidationViews.color f.ValidationErrors (nameof(f.FormData.Capacity))
                    prop.onTextChange (fun x -> { f.FormData with Capacity = int x } |> LessonsFormChanged |> dispatch)
                    prop.valueOrDefault (f.FormData.Capacity)
                ]
            ]
            ValidationViews.help f.ValidationErrors (nameof(f.FormData.Capacity))
        ]
        Bulma.field.div [
            Bulma.label "Odhlášení před začátkem (hodin)"
            Bulma.fieldBody [
                Bulma.input.number [
                    ValidationViews.color f.ValidationErrors (nameof(f.FormData.CancellableBeforeStart))
                    prop.onTextChange (fun x -> { f.FormData with CancellableBeforeStart = float x |> TimeSpan.FromHours } |> LessonsFormChanged |> dispatch)
                    prop.valueOrDefault (f.FormData.CancellableBeforeStart.TotalHours)
                ]
            ]
            ValidationViews.help f.ValidationErrors (nameof(f.FormData.CancellableBeforeStart))
        ]
        Bulma.field.div [
            Bulma.fieldBody [
                Bulma.button.button [
                    color.isPrimary
                    prop.text "Přidat lekce"
                    if f.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    prop.onClick (fun _ -> CreateLessons |> dispatch)
                ]
            ]
        ]
    ]

let workshopsForm (f:ValidatedForm<Request.CreateWorkshops>) dispatch =
    Html.div [
        Bulma.field.div [
            Bulma.label "Termíny"
            Bulma.fieldBody [
                f.FormData.Dates
                |> List.map DateTimeOffset.toCzDate
                |> String.concat ", "
                |> Html.span
            ]
            ValidationViews.help f.ValidationErrors (nameof(f.FormData.Dates))
        ]
        
        Bulma.field.div [
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
        Bulma.field.div [
            Bulma.label "Název"
            Bulma.fieldBody [
                Bulma.input.text [
                    ValidationViews.color f.ValidationErrors (nameof(f.FormData.Name))
                    prop.onTextChange (fun x -> { f.FormData with Name = x } |> WorkshopsFormChanged |> dispatch)
                    prop.valueOrDefault (f.FormData.Name)
                ]
            ]
            ValidationViews.help f.ValidationErrors (nameof(f.FormData.Name))
        ]
        Bulma.field.div [
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
        
        Bulma.field.div [
            Bulma.fieldBody [
                Bulma.button.button [
                    color.isPrimary
                    prop.text "Přidat workshopy"
                    if f.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    prop.onClick (fun _ -> CreateWorkshops |> dispatch)
                ]
            ]
        ]
    ]
    
let inFormQuickView dispatch (header:string) (content:ReactElement) =    
    QuickView.quickview [
        quickview.isActive
        prop.children [
            QuickView.header [
                Html.div header
                Bulma.delete [ prop.onClick (fun _ -> None |> SelectActiveForm |> dispatch) ]
            ]
            QuickView.body [
                QuickView.block [
                    prop.style [ style.padding 15 ]
                    prop.children content
                ]
            ]
        ]
    ]
    
let inItemQuickView dispatch (header:string) (content:ReactElement) =    
    QuickView.quickview [
        quickview.isActive
        prop.children [
            QuickView.header [
                Html.div header
                Bulma.delete [ prop.onClick (fun _ -> None |> SetActiveLesson |> dispatch) ]
            ]
            QuickView.body [
                QuickView.block [
                    prop.style [ style.padding 15 ]
                    prop.children content
                ]
            ]
        ]
    ]

let getTag (res:'a list) capacity isCancelled =
    let tagColor =
        match res.Length with
        | 0 -> color.isBlack
        | x when x > 0 && x < capacity -> color.isWarning
        | _ -> color.isSuccess         
    
    let tagText =
        if isCancelled then "Lekce je zrušena"
        else sprintf "%i / %i" res.Length capacity
    
    Bulma.tag [ tagColor; prop.text tagText ]    

let workshopDiv dispatch (workshop:Queries.Workshop) =
    Html.div [
        prop.className "workshop"
        prop.children [
            Html.div [
                prop.className "time"
                prop.text (sprintf "%s - %s" (workshop.StartDate |> DateTimeOffset.toCzTime) (workshop.EndDate |> DateTimeOffset.toCzTime))
            ]
            Html.div [
                prop.className "name"
                prop.text workshop.Name
            ]
            Html.div [
                Bulma.button.button [
                    color.isLight
                    prop.text "Detail"
                    prop.onClick (fun _ -> workshop |> Some |> SetActiveWorkshop |> dispatch)
                ]
            ]
        ]
    ]

let lessonDiv dispatch (lesson:Queries.Lesson) =
    Html.div [
        prop.classes [ "lesson"; if lesson.IsCancelled then "cancelled" ]
        prop.children [
            Html.div [
                prop.className "time"
                prop.children [
                    Html.text (sprintf "%s - %s" (lesson.StartDate |> DateTimeOffset.toCzTime) (lesson.EndDate |> DateTimeOffset.toCzTime))
                    Html.div [ getTag lesson.Reservations lesson.Capacity lesson.IsCancelled ]
                ]
            ]
            Html.div [
                prop.className "name"
                prop.text lesson.Name
            ]
            Html.div [
                Bulma.button.button [
                    color.isLight
                    prop.text "Detail"
                    prop.onClick (fun _ -> lesson |> Some |> SetActiveLesson |> dispatch)
                ]
            ]
        ]
    ]

let col (lessons:Queries.Lesson list) (workshops:Queries.Workshop list) (date:DateTimeOffset) dispatch =
    Html.td [
        Html.div (lessons |> List.map (lessonDiv dispatch))
        Html.div (workshops |> List.map (workshopDiv dispatch))
    ]

let row model dispatch dates =
    let getLessonsForDate (date:DateTimeOffset) =
        model.Lessons
        |> List.filter (fun x -> x.StartDate.Date = date.Date)

    let getWorkshopsForDate (date:DateTimeOffset) =
        model.Workshops
        |> List.filter (fun x -> x.StartDate.Date = date.Date)

    dates
    |> List.map (fun date ->
        let lsns = date |> getLessonsForDate
        let wrksps = date |> getWorkshopsForDate
        col lsns wrksps date dispatch
    )
    |> (fun x ->
        Html.tr [
            prop.className "day"
            prop.children x
        ]
    )

let formQuickView model dispatch =
    match model.ActiveForm with
    | Some LessonsForm -> lessonsForm model.LessonsForm dispatch |> inFormQuickView dispatch "Lekce"
    | Some WorkshopsForm -> workshopsForm model.WorkshopsForm dispatch |> inFormQuickView dispatch "Workshopy"
    | None -> Html.none

let lessonItemForm (dispatch:ActiveLessonMsg -> unit) (l:ActiveLessonModel) =
    
    let reserved =
        let rows = 
            l.Lesson.Reservations
            |> List.map (fun (x,p) ->
                Html.tr [
                    Html.td (sprintf "%s %s" x.FirstName x.LastName)
                    Html.td (p |> Queries.paymentToText)
                ]
            )
            
        Bulma.table [
            table.isNarrow
            table.isStriped
            prop.children [
                Html.tbody rows
            ]
        ]
        
    let actions =
        Html.div [
            Bulma.buttons [
                if Domain.canLessonBeCancelled l.Lesson.IsCancelled l.Lesson.StartDate then
                    Bulma.button.button [
                        color.isWarning
                        prop.text "Zrušit lekci"
                        prop.onClick (fun _ -> CancelLesson |> dispatch)
                        if l.CancelLessonForm.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    ]
                if Domain.canLessonBeDeleted l.Lesson.StartDate then                    
                    Bulma.button.button [
                        color.isDanger
                        prop.text "Smazat lekci"
                        prop.onClick (fun _ -> DeleteLesson |> dispatch)
                        if l.DeleteLessonForm.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    ]
            ]
        ]
    
    Html.div [
        Bulma.field.div [
            Bulma.label "Název"
            Bulma.input.text [
                ValidationViews.color l.ChangeDescriptionForm.ValidationErrors (nameof(l.ChangeDescriptionForm.FormData.Name))
                prop.onTextChange (fun x -> { l.ChangeDescriptionForm.FormData with Name = x } |> ChangeLessonDescriptionFormChanged |> dispatch)
                prop.valueOrDefault l.ChangeDescriptionForm.FormData.Name
            ]
            ValidationViews.help l.ChangeDescriptionForm.ValidationErrors (nameof(l.ChangeDescriptionForm.FormData.Name))
        ]
        Bulma.field.div [
            Bulma.label "Popis"
            Bulma.fieldBody [
                Bulma.textarea [
                    ValidationViews.color l.ChangeDescriptionForm.ValidationErrors (nameof(l.ChangeDescriptionForm.FormData.Description))
                    prop.onTextChange (fun x -> { l.ChangeDescriptionForm.FormData with Description = x } |> ChangeLessonDescriptionFormChanged |> dispatch)
                    prop.valueOrDefault l.ChangeDescriptionForm.FormData.Description
                ]
            ]
            ValidationViews.help l.ChangeDescriptionForm.ValidationErrors (nameof(l.ChangeDescriptionForm.FormData.Description))
        ]
        Bulma.field.div [ Bulma.fieldBody [
            Bulma.button.button [
                color.isInfo
                prop.text "Upravit popis"
                if l.ChangeDescriptionForm.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                prop.onClick (fun _ -> ChangeLessonDescription |> dispatch)
            ]
        ] ]
        
        Bulma.field.div [ Bulma.label "Datum"; Bulma.fieldBody (l.Lesson.StartDate |> DateTimeOffset.toCzDate) ]
        Bulma.field.div [ Bulma.label "Čas"; Bulma.fieldBody (sprintf "%s - %s" (l.Lesson.StartDate |> DateTimeOffset.toCzTime) (l.Lesson.EndDate |> DateTimeOffset.toCzTime)) ]
        Bulma.field.div [ Bulma.label "Kapacita"; Bulma.fieldBody (l.Lesson.Capacity) ]
        Bulma.field.div [ Bulma.label "Odhlášení před začátkem (hodin)"; Bulma.fieldBody (string l.Lesson.CancellableBeforeStart.TotalHours) ]
        Bulma.field.div [ Bulma.label "Přihlášení"; Bulma.fieldBody reserved ]
        Bulma.field.div [ Bulma.label "Akce"; Bulma.fieldBody actions ]
    ]

let workshopItemForm (dispatch:ActiveWorkshopMsg -> unit) (w:ActiveWorkshopModel) =
        
    let actions =
        Html.div [
            Bulma.buttons [
                Bulma.button.button [
                    color.isDanger
                    prop.text "Smazat workshop"
                    prop.onClick (fun _ -> DeleteWorkshop |> dispatch)
                    if w.DeleteWorkshopForm.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                ]
            ]
        ]
    
    Html.div [
        Bulma.field.div [ Bulma.label "Název"; Bulma.fieldBody w.Workshop.Name ]
        Bulma.field.div [ Bulma.label "Popis"; Bulma.fieldBody (Html.text [ w.Workshop.Description |> eolToBr |> prop.dangerouslySetInnerHTML ]) ]
        Bulma.field.div [ Bulma.label "Datum"; Bulma.fieldBody (w.Workshop.StartDate |> DateTimeOffset.toCzDate) ]
        Bulma.field.div [ Bulma.label "Čas"; Bulma.fieldBody (sprintf "%s - %s" (w.Workshop.StartDate |> DateTimeOffset.toCzTime) (w.Workshop.EndDate |> DateTimeOffset.toCzTime)) ]
        Bulma.field.div [ Bulma.label "Akce"; Bulma.fieldBody actions ]
    ]

let activeItemQuickView (model:Model) (dispatch:Msg -> unit) =
    match model.ActiveItemModel with
    | Some (Lesson l) -> l |> lessonItemForm (ActiveLessonMsg >> Msg.ActiveItemMsg >> dispatch) |> inItemQuickView dispatch l.Lesson.Name
    | Some (Workshop w) -> w |> workshopItemForm (ActiveWorkshopMsg >> Msg.ActiveItemMsg >> dispatch) |> inItemQuickView dispatch w.Workshop.Name
    | None -> Html.none

let view = React.functionComponent(fun () ->
    let model, dispatch = React.useElmish(State.init, State.update, [| |])    

    let dates =
        model.WeekOffset
        |> DateRange.getDateRangeForWeekOffset
        |> DateRange.dateRangeToDays
    
    Html.div [
        formQuickView model dispatch
        activeItemQuickView model dispatch
        Bulma.table [
            table.isFullWidth
            table.isBordered
            ++ prop.className "table-calendar"
            prop.children [
                Html.tbody [
                    navigationRow model dispatch
                    dates |> headerRow model dispatch
                    dates |> row model dispatch
                ]
            ]
        ]
    ]
)