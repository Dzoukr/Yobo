module Yobo.Client.Admin.Users.View

open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Fulma
open System
open Yobo.Client.Admin.Users.Domain
open Yobo.Shared.Domain
open Fulma.Extensions.Wikiki
open Yobo.Shared.Extensions

let private userRow dispatch (u:User) =
    let activated =
        match u.Activated with
        | Some a -> a.ToString("dd. MM. yyyy") |> str
        | None -> Tag.tag [ Tag.Color IsWarning ] [ str "Neaktivní" ]

    let expires =
        match u.CreditsExpiration with
        | Some a -> a.ToString("dd. MM. yyyy") |> str
        | None -> str "-"

    let addCreditBtn =
        match u.Activated with
        | Some _ ->
            Button.button 
                [ Button.Color IsPrimary; Button.IsFullWidth; Button.OnClick (fun _ -> u.Id |> ToggleAddCreditsForm |> dispatch)  ]
                [ str "Přidat kredity" ]
        | None -> str ""

    tr [] [
        td [] [ str u.LastName]
        td [] [ str u.FirstName]
        td [] [ str u.Email]
        td [] [ activated ]
        td [] [ u.Credits |> string |> str ]
        td [] [ expires ]
        td [] [ addCreditBtn ]
    ]

let private showForm dispatch (state:State) (user:User option) =
    match user with
    | Some u ->
        let toDateTimeOffset = Option.map (fun (x:DateTime) -> DateTimeOffset(x).EndOfTheDay())
        let calendar =
            let opts = { 
                Yobo.Client.Components.Calendar.Options.Default 
                    with 
                        StartDate = state.ExpirationDate |> Option.map (fun x -> x.Date)
                        DisplayMode = Yobo.Client.Components.Calendar.DisplayMode.Inline
                        MinimumDate = Some (DateTimeOffset.Now.Date.AddDays 7.)
                        WeekStart = 1
                        Lang = "cs"
                }
            Yobo.Client.Components.Calendar.view opts "myCalc" (fst >> toDateTimeOffset >> CalendarChanged >> dispatch)

        let lbl txt = Label.label [] [ str txt ]

        let form = div [] [
            Field.div [ ] [
                lbl "Uživatel"
                div [] [ sprintf "%s %s" u.FirstName u.LastName |> str]
            ]
            Field.div [ ] [
                lbl "Počet kreditů"
                Control.div [ ] [
                    Input.number [
                        Input.Option.DefaultValue <| state.Credits.ToString()
                        Input.Option.Props [ Props.Min 1 ];
                        Input.Option.OnChange (fun e -> !!e.target?value |> int |> (CreditsChanged >> dispatch))
                    ]
                ] 
            ]
            Field.div [ ] [
                lbl "Datum expirace"
                Control.div [ ] [ calendar ] 
            ]
            Field.div [ Field.IsGrouped; Field.IsGroupedCentered ]
                [ Control.div [ ]
                    [ Button.button [
                        Button.OnClick (fun _ -> SubmitForm |> dispatch)
                        Button.Color IsPrimary
                        Button.Disabled (state.Credits < 1 || state.ExpirationDate.IsNone) ]
                        [ str "Přidat kredity" ] ]
                ] 
        ]
            
        div [ ] [
            Quickview.quickview [ Quickview.IsActive true ] [
                Quickview.header [ ] [
                    Quickview.title [ ] [ str "Přidat kredity" ]
                    Delete.delete [ Delete.OnClick (fun _ -> u.Id |> ToggleAddCreditsForm |> dispatch) ] [ ]
                ]
                Quickview.body [ ]
                    [ form ]
                ]
        ]
    | None -> str ""

let private loadingRow =
    tr [] [
        td [ ColSpan 7 ] [
            i [ ClassName "fas fa-circle-notch fa-spin" ] []
        ]
    ]

let render (state : State) (dispatch : Msg -> unit) =
    let rows = if state.UsersLoading then [ loadingRow ] else state.Users |> List.map (userRow dispatch)
    let table =
        Table.table [ Table.IsHoverable ] [
            thead [ ] [
                tr [ ] [
                    th [ ] [ str "Příjmení" ]
                    th [ ] [ str "Jméno" ]
                    th [ ] [ str "Email" ]
                    th [ ] [ str "Datum aktivace" ]
                    th [ ] [ str "Kredity" ]
                    th [ ] [ str "Datum expirace" ]
                    th [ ] [ str "" ]
                ]
            ] 
            tbody [ ] rows
        ]
    div [] [
        table
        state.Users |> List.tryFind (fun x -> Some x.Id = state.SelectedUserId) |> showForm dispatch state
    ]