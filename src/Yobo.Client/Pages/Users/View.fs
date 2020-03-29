module Yobo.Client.Pages.Users.View

open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open System
open Domain
open Feliz
open Feliz.Bulma
open Feliz.Bulma.QuickView
open Feliz.Bulma.Calendar
open Yobo.Client.SharedView
open Yobo.Shared.Errors
open Yobo.Shared.DateTime
open Yobo.Shared.Core.Admin.Domain.Queries

let private userRow dispatch (u:User) =
    let activated =
        match u.Activated with
        | Some a -> a.ToString("dd. MM. yyyy") |> Html.text
        | None -> Bulma.tag [ tag.isWarning; prop.text "Neaktivní" ]

    let expires =
        match u.CreditsExpiration with
        | Some a -> a.ToString("dd. MM. yyyy") |> Html.text
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
                prop.text "Prodloužit platnost"
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
                    //ValidationViews.color model.Form.ValidationErrors (nameof(model.Form.FormData.LastName))
                    //prop.onTextChange (fun x -> { model.Form.FormData with LastName = x } |> FormChanged |> dispatch)
                    //prop.valueOrDefault model.Form.FormData.LastName
                ]
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
                        Fable.Core.JS.console.log(x)
                    )
                ]
            ]
        ]
        Bulma.field [
            Bulma.fieldBody [
                Bulma.button [
                    button.isPrimary
                    prop.text "Přidat kredity"
                    //prop.onClick (fun _ -> Reset |> dispatch)
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
    
//let private showForm dispatch (state:State) (user:User option) =
//    match user with
//    | Some u ->
//        let toDateTimeOffset = Option.map (fun (x:DateTime) -> DateTimeOffset(x).EndOfTheDay())
//        let calendar =
//            let opts = { 
//                Yobo.Client.Components.Calendar.Options.Default 
//                    with 
//                        StartDate = state.ExpirationDate |> Option.map (fun x -> x.Date)
//                        DisplayMode = Yobo.Client.Components.Calendar.DisplayMode.Inline
//                        MinimumDate = Some (DateTimeOffset.Now.Date.AddDays 7.)
//                        WeekStart = 1
//                        Lang = "cs"
//                }
//            Yobo.Client.Components.Calendar.view opts "myCalc" (fst >> toDateTimeOffset >> CalendarChanged >> dispatch)
//
//        let lbl txt = Label.label [] [ str txt ]
//
//        let form = div [] [
//            Field.div [ ] [
//                lbl "Uživatel"
//                div [] [ sprintf "%s %s" u.FirstName u.LastName |> str]
//            ]
//            Field.div [ ] [
//                lbl "Počet kreditů"
//                Control.div [ ] [
//                    Input.number [
//                        Input.Option.DefaultValue <| state.Credits.ToString()
//                        Input.Option.Props [ Props.Min 1 ];
//                        Input.Option.OnChange (fun e -> !!e.target?value |> int |> (CreditsChanged >> dispatch))
//                    ]
//                ] 
//            ]
//            Field.div [ ] [
//                lbl "Datum expirace"
//                Control.div [ ] [ calendar ] 
//            ]
//            Field.div [ Field.IsGrouped; Field.IsGroupedCentered ]
//                [ Control.div [ ]
//                    [ Button.button [
//                        Button.OnClick (fun _ -> SubmitForm |> dispatch)
//                        Button.Color IsPrimary
//                        Button.Disabled (state.Credits < 1 || state.ExpirationDate.IsNone) ]
//                        [ str "Přidat kredity" ] ]
//                ] 
//        ]
//            
//        div [ ] [
//            Quickview.quickview [ Quickview.IsActive true ] [
//                Quickview.header [ ] [
//                    Quickview.title [ ] [ str "Přidat kredity" ]
//                    Delete.delete [ Delete.OnClick (fun _ -> u.Id |> ToggleAddCreditsForm |> dispatch) ] [ ]
//                ]
//                Quickview.body [ ]
//                    [ form ]
//                ]
//        ]
//    | None -> str ""

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
    
    Html.div [
        table
        quickviewAddCredits
    ]        