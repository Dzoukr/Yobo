module Yobo.Client.Admin.View

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


let private userRow dispatch (u:Admin.Domain.User) =
    let activated =
        match u.ActivatedUtc with
        | Some a -> a.ToString("dd. MM. yyyy") |> str
        | None -> Tag.tag [ Tag.Color IsWarning ] [ TextValue.Innactive |> Locale.toTitleCz |> str ]

    let addCreditBtn =
        match u.ActivatedUtc with
        | Some _ ->
            Button.button 
                [ Button.Color IsPrimary; Button.IsFullWidth; Button.OnClick (fun _ -> u.Id |> ToggleAddCreditsForm |> dispatch)  ]
                [ TextValue.AddCredits |> Locale.toTitleCz |> str ]
        | None -> str ""

    tr [] [
        td [] [ str u.LastName]
        td [] [ str u.FirstName]
        td [] [ str u.Email]
        td [] [ activated ]
        td [] [ addCreditBtn ]
    ]

let private showForm dispatch (state:State) (user:Admin.Domain.User option) =
    match user with
    | Some u ->
        
        let calendar =
            let opts = { 
                Yobo.Client.Components.Calendar.Options.Default 
                    with 
                        StartDate = state.AddCreditsForm.ExpirationDate |> Option.orElse (DateTime.Now.AddMonths(3) |> Some)
                        DisplayMode = Yobo.Client.Components.Calendar.DisplayMode.Inline
                        MinimumDate = Some (DateTime.Now.AddDays 7.)
                        WeekStart = 1
                        Lang = "cs"
                }
            Yobo.Client.Components.Calendar.view opts "myCalc" (fst >> CalendarChanged >> AddCreditsFormMsg >> dispatch)

        let lbl txt = Label.label [] [ str (Locale.toTitleCz txt) ]

        let form = div [] [
            Field.div [ ] [
                lbl User
                div [] [ sprintf "%s %s" u.FirstName u.LastName |> str]
            ]
            Field.div [ ] [
                lbl CreditsCount
                Control.div [ ] [
                    Input.number [
                        Input.Option.DefaultValue <| state.AddCreditsForm.Credits.ToString()
                        Input.Option.Props [ Props.Min 1 ];
                        Input.Option.OnChange (fun e -> !!e.target?value |> int |> (CreditsChanged >> AddCreditsFormMsg >> dispatch))
                    ]
                ] 
            ]
            Field.div [ ] [
                lbl ExpirationDate
                Control.div [ ] [ calendar ] 
            ]
            Field.div [ Field.IsGrouped; Field.IsGroupedCentered ]
                [ Control.div [ ]
                    [ Button.button [
                        Button.OnClick (fun _ -> SubmitForm |> AddCreditsFormMsg |> dispatch)
                        Button.Color IsPrimary
                        Button.Disabled (state.AddCreditsForm.Credits < 1 || state.AddCreditsForm.ExpirationDate.IsNone) ]
                        [ str (AddCredits |> Locale.toTitleCz) ] ]
                ] 
        ]
            
        div [ ] [
            Quickview.quickview [ Quickview.IsActive true ] [
                Quickview.header [ ] [
                    Quickview.title [ ] [ Text.AddCredits |> Locale.toTitleCz |> str ]
                    Delete.delete [ Delete.OnClick (fun _ -> u.Id |> ToggleAddCreditsForm |> dispatch) ] [ ]
                ]
                Quickview.body [ ]
                    [ form ]
                ]
        ]
    | None -> str ""

let render (state : State) (dispatch : Msg -> unit) =
    let rows = state.Users |> List.map (userRow dispatch)
    let table =
        Table.table [ Table.IsHoverable ] [
            thead [ ] [
                tr [ ] [
                    th [ ] [ str (Text.TextValue.LastName |> Locale.toTitleCz) ]
                    th [ ] [ str (Text.TextValue.FirstName |> Locale.toTitleCz) ]
                    th [ ] [ str (Text.TextValue.Email |> Locale.toTitleCz) ]
                    th [ ] [ str (Text.TextValue.ActivationDate |> Locale.toTitleCz) ]
                    th [ ] [ str "" ]
                ]
            ] 
            tbody [ ] rows
        ]
    div [] [
        table
        state.Users |> List.tryFind (fun x -> Some x.Id = state.AddCreditsForm.SelectedUserId) |> showForm dispatch state
    ]