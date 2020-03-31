module Yobo.Client.Pages.Users.View

open System
open Domain
open Feliz
open Feliz.Bulma
open Feliz.Bulma.QuickView
open Feliz.Bulma.Calendar
open Yobo.Client.SharedView
open Yobo.Shared.DateTime
open Yobo.Shared.Core.Admin.Domain.Queries

let private userRow dispatch (u:User) =
    let activated =
        match u.Activated with
        | Some a -> a |> DateTimeOffset.toCzDate |> Html.text
        | None -> Bulma.tag [ tag.isWarning; prop.text "Neaktivní" ]

    let expires =
        match u.CreditsExpiration with
        | Some a -> a |> DateTimeOffset.toCzDate |> Html.text
        | None -> Html.text "-"

    let addCreditBtn =
        if u.Activated.IsSome then
            Bulma.button [
                button.isPrimary
                prop.onClick (fun _ -> u.Id |> Some |> ShowAddCreditsForm |> dispatch)
                prop.text "Přidat kredity"
            ]
        else Html.none
        
    let prolongBtn =
        if u.Activated.IsSome && u.Credits > 0 then
            Bulma.button [
                prop.style [ style.marginLeft 5 ]
                button.isLight
                prop.onClick (fun _ -> u.Id |> Some |> ShowSetExpirationForm |> dispatch)
                prop.text "Nastavit platnost"
            ]
        else Html.none
    
    Html.tr [
        Html.td u.LastName
        Html.td u.FirstName
        Html.td u.Email
        Html.td activated
        Html.td (u.Credits |> string)
        Html.td expires
        Html.td [ addCreditBtn; prolongBtn ]
    ]

let addCreditsQuickView (model:Model) dispatch user =
    
    let form = [
        Bulma.field [
            Bulma.label "Uživatel"
            Bulma.fieldBody [
                Html.div (sprintf "%s %s" user.FirstName user.LastName)
            ]
        ]
        Bulma.field [
            Bulma.label "Počet kreditů"
            Bulma.fieldBody [
                Bulma.numberInput [
                    ValidationViews.color model.AddCreditsForm.ValidationErrors (nameof(model.AddCreditsForm.FormData.Credits))
                    prop.onTextChange (fun x -> { model.AddCreditsForm.FormData with Credits = int x } |> AddCreditsFormChanged |> dispatch)
                    prop.valueOrDefault model.AddCreditsForm.FormData.Credits
                ]
            ]
            ValidationViews.help model.AddCreditsForm.ValidationErrors (nameof(model.AddCreditsForm.FormData.Credits))
        ]
        Bulma.field [
            Bulma.label "Datum expirace"
            Bulma.fieldBody [
                Calendar.calendar [
                    prop.id "expCal"
                    calendar.options [
                        calendar.options.type'.date
                        calendar.options.isRange false
                        calendar.options.displayMode.inline'
                        calendar.options.weekStart DayOfWeek.Monday
                        calendar.options.showFooter false
                        calendar.options.minDate DateTime.UtcNow
                        calendar.options.lang "cs"
                    ]
                    calendar.onValueSelected (fun x ->
                        match x with
                        | SingleValue (SingleValue.Date (Some v)) ->
                            { model.AddCreditsForm.FormData with Expiration = DateTimeOffset(v) |> DateTimeOffset.endOfTheDay }
                            |> AddCreditsFormChanged
                            |> dispatch
                        | _ -> ()
                    )
                ]
            ]
            ValidationViews.help model.AddCreditsForm.ValidationErrors (nameof(model.AddCreditsForm.FormData.Expiration))
        ]
        Bulma.field [
            Bulma.fieldBody [
                Bulma.button [
                    button.isPrimary
                    prop.text "Přidat kredity"
                    if model.AddCreditsForm.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    prop.onClick (fun _ -> AddCredits |> dispatch)
                ]
            ]
        ]
    ]
    
    QuickView.quickview [
        quickview.isActive
        prop.children [
            QuickView.header [
                Html.div "Přidat kredity"
                Bulma.delete [ prop.onClick (fun _ -> None |> ShowAddCreditsForm |> dispatch) ]
            ]
            QuickView.body [
                QuickView.block [
                    prop.style [ style.padding 15 ]
                    prop.children form
                ]
            ]
        ]
    ]

let setExpirationQuickView (model:Model) dispatch user =
    
    let form = [
        Bulma.field [
            Bulma.label "Uživatel"
            Bulma.fieldBody [
                Html.div (sprintf "%s %s" user.FirstName user.LastName)
            ]
        ]
        Bulma.field [
            Bulma.label "Datum expirace"
            Bulma.fieldBody [
                Calendar.calendar [
                    prop.id "expCal"
                    calendar.options [
                        calendar.options.type'.date
                        calendar.options.isRange false
                        calendar.options.displayMode.inline'
                        calendar.options.weekStart DayOfWeek.Monday
                        calendar.options.showFooter false
                        calendar.options.minDate DateTime.UtcNow
                        calendar.options.lang "cs"
                    ]
                    calendar.onValueSelected (fun x ->
                        match x with
                        | SingleValue (SingleValue.Date (Some v)) ->
                            { model.SetExpirationForm.FormData with Expiration = DateTimeOffset(v) |> DateTimeOffset.endOfTheDay }
                            |> SetExpirationFormChanged
                            |> dispatch
                        | _ -> ()
                    )
                ]
            ]
            ValidationViews.help model.SetExpirationForm.ValidationErrors (nameof(model.SetExpirationForm.FormData.Expiration))
        ]
        Bulma.field [
            Bulma.fieldBody [
                Bulma.button [
                    button.isPrimary
                    prop.text "Nastavit platnost"
                    if model.SetExpirationForm.IsLoading then yield! [ button.isLoading; prop.disabled true ]
                    prop.onClick (fun _ -> SetExpiration |> dispatch)
                ]
            ]
        ]
    ]
    
    QuickView.quickview [
        quickview.isActive
        prop.children [
            QuickView.header [
                Html.div "Nastavit platnost"
                Bulma.delete [ prop.onClick (fun _ -> None |> ShowSetExpirationForm |> dispatch) ]
            ]
            QuickView.body [
                QuickView.block [
                    prop.style [ style.padding 15 ]
                    prop.children form
                ]
            ]
        ]
    ]

let private loadingRow =
    Html.tr [
        prop.colSpan 7
        prop.children [
            Html.td [
                Html.faIcon "fas fa-circle-notch fa-spin"
            ]
        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    let rows = if model.UsersLoading then [ loadingRow ] else model.Users |> List.map (userRow dispatch)
    let table =
        Bulma.table [
            Bulma.table.isHoverable
            Bulma.table.isFullwidth
            prop.children [
                Html.thead [
                    Html.tr [
                        Html.th "Příjmení"
                        Html.th "Jméno"
                        Html.th "Email"
                        Html.th "Datum aktivace"
                        Html.th "Kredity"
                        Html.th "Datum expirace"
                        Html.th ""
                    ]
                ]
                Html.tbody rows
            ]
        ]
    
    let quickviewAddCredits =
        model.AddCreditsSelectedUser
        |> Option.bind (fun uId -> model.Users |> List.tryFind (fun x -> x.Id = uId))
        |> Option.map (addCreditsQuickView model dispatch)
        |> Option.defaultValue Html.none
    
    let quickviewSetExpiration =
        model.SetExpirationSelectedUser
        |> Option.bind (fun uId -> model.Users |> List.tryFind (fun x -> x.Id = uId))
        |> Option.map (setExpirationQuickView model dispatch)
        |> Option.defaultValue Html.none
    
    Html.div [
        table
        quickviewAddCredits
        quickviewSetExpiration
    ]        