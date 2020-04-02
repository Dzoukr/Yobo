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
open Yobo.Shared.Core.Admin.Domain

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
            Bulma.dropdown [
                dropdown.isHoverable
                prop.children [
                    Bulma.dropdownTrigger [
                        Bulma.button [
                            button.isPrimary
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
                            Bulma.dropdownItem [
                                Html.a [
                                    prop.text "Lekce"; prop.onClick (fun _ -> SelectActiveForm (Some LessonsForm) |> dispatch)
                                ]
                            ]
                            Bulma.dropdownItem [
                                Html.a [
                                    prop.text "Workshopy"; prop.onClick (fun _ -> SelectActiveForm (Some WorkshopsForm) |> dispatch)
                                ]
                            ]
                            Bulma.dropdownItem [
                                Html.a [
                                    prop.text "Online lekce"; prop.onClick (fun _ -> SelectActiveForm (Some OnlinesForm) |> dispatch)
                                ]
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
    
let lessonsForm (f:ValidatedForm<Request.CreateLessons>) dispatch =
    
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

let workshopsForm (f:ValidatedForm<Request.CreateWorkshops>) dispatch =
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
    
let onlinesForm (f:ValidatedForm<Request.CreateOnlineLessons>) dispatch =
    
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
    
let inQuickView dispatch (header:string) (content:ReactElement) =    
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

let getTag (res:'a list) capacity isCancelled =
    let tagColor =
        match res.Length with
        | 0 -> tag.isBlack
        | x when x > 0 && x < capacity -> tag.isWarning
        | _ -> tag.isSuccess         
    
    let tagText =
        if isCancelled then "Lekce je zrušena"
        else sprintf "%i / %i" res.Length capacity
    
    Bulma.tag [ tagColor; prop.text tagText ]    

let workshopDiv (workshop:Queries.Workshop) =
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
                Bulma.button [
                    button.isLight
                    prop.text "Detail"
                ]
            ]
        ]
    ]

let lessonDiv (lesson:Queries.Lesson) =
    Html.div [
        prop.className "lesson"
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
                Bulma.button [
                    button.isLight
                    prop.text "Detail"
                ]
            ]
        ]
    ]

let onlineLessonDiv (lesson:Queries.OnlineLesson) =
      
    Html.div [
        prop.className "online-lesson"
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
                Bulma.button [
                    button.isLight
                    prop.text "Detail"
                ]
            ]
        ]
    ]

let col (lessons:Queries.Lesson list) (workshops:Queries.Workshop list) (onlines:Queries.OnlineLesson list) (date:DateTimeOffset) dispatch =
    Html.td [
        Html.div (workshops |> List.map workshopDiv)
        Html.div (lessons |> List.map lessonDiv)
        Html.div (onlines |> List.map onlineLessonDiv)
    ]

let row model dispatch dates =
    let getLessonsForDate (date:DateTimeOffset) =
        model.Lessons
        |> List.filter (fun x -> x.StartDate.Date = date.Date)

    let getWorkshopsForDate (date:DateTimeOffset) =
        model.Workshops
        |> List.filter (fun x -> x.StartDate.Date = date.Date)

    let getOnlineLessonsForDate (date:DateTimeOffset) =
        model.Onlines
        |> List.filter (fun x -> x.StartDate.Date = date.Date)

    dates
    |> List.map (fun date ->
        let lsns = date |> getLessonsForDate
        let wrksps = date |> getWorkshopsForDate
        let onlns = date |> getOnlineLessonsForDate
        col lsns wrksps onlns date dispatch
    )
    |> (fun x ->
        Html.tr [
            prop.className "day"
            prop.children x
        ]
    )

let formQuickView model dispatch =
    match model.ActiveForm with
    | Some LessonsForm -> lessonsForm model.LessonsForm dispatch |> inQuickView dispatch "Lekce"
    | Some WorkshopsForm -> workshopsForm model.WorkshopsForm dispatch |> inQuickView dispatch "Workshopy"
    | Some OnlinesForm -> onlinesForm model.OnlinesForm dispatch |> inQuickView dispatch "Online lekce"
    | None -> Html.none
    
let view (model:Model) (dispatch: Msg -> unit) =

    let dates =
        model.WeekOffset
        |> DateRange.getDateRangeForWeekOffset
        |> DateRange.dateRangeToDays
    
    Html.div [
        formQuickView model dispatch
        Bulma.table [
            table.isFullwidth
            table.isBordered
            ++ prop.className "table-calendar"
            prop.children [
                navigationRow model dispatch
                dates |> headerRow model dispatch
                dates |> row model dispatch
            ]
        ]
    ]