module Yobo.Client.Pages.Users.View

open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open System
open Domain
open Feliz
open Feliz.Bulma
open Yobo.Client.SharedView
open Yobo.Shared.Domain
open Yobo.Shared.DateTime
open Yobo.Shared.Users.Domain.Queries

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
                //prop.onClick (fun _ -> u.Id |> ToggleAddCreditsForm |> dispatch)
                prop.text "Přidat kredity"
            ]
        else Html.none
        
    let prolongBtn =
        if u.Activated.IsSome && u.Credits > 0 then
            Bulma.button [
                prop.style [ style.marginLeft 5 ]
                button.isLight
                //prop.onClick (fun _ -> u.Id |> ToggleAddCreditsForm |> dispatch)
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
        prop.children [ Html.faIcon "fas fa-circle-notch fa-spin" ]
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
    
    Html.div [
        table
        //model.Users 
        //state.Users |> List.tryFind (fun x -> Some x.Id = state.SelectedUserId) |> showForm dispatch state
    ]        