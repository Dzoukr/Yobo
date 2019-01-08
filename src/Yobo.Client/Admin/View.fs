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

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import

[<Emit("bulmaCalendar.attach($0, $1)[0].on('date:selected', date => { $2(date) });")>]
let attachCalendarScript (selector: string) (opts:obj) (fn:obj -> unit) : unit = jsNative

let attachCalendar (fn: DateTime option * DateTime option -> unit) (elm: Fable.Import.Browser.Element)  =
    let opts = jsOptions(fun x ->
        x?weekStart <- 1
        x?lang <- "cs"
    )

    let toDateTime obj =
        match obj with
        | null -> None
        | x -> x?toISOString() |> DateTime.Parse |> Some

    if elm |> isNull |> not then
        let selector = sprintf "[id=\"%s\"]" elm.id
        if elm.nextSibling |> isNull then 
            attachCalendarScript selector opts (fun d ->
                let s = d?start |> toDateTime
                let e = d?``end`` |> toDateTime
                fn (s, e)
            )

let private showForm dispatch state (user:Admin.Domain.User option) =
    match user with
    | Some u ->

        let root =
            Input.date [
                Input.Option.Id "mujcalc"
                Input.Option.Ref(attachCalendar (fun t -> t |> fst |> CalendarChanged |> dispatch ));
                Input.Option.Props [  Data("display-mode","inline"); ]
            ]

        div [ ]
            [ Quickview.quickview [ Quickview.IsActive true ]
                    [ Quickview.header [ ]
                        [ Quickview.title [ ] [ Text.AddCredits |> Locale.toTitleCz |> str ]
                          Delete.delete [ Delete.OnClick (fun _ -> u.Id |> ToggleAddCreditsForm |> dispatch) ] [ ] ]
                      Quickview.body [ ]
                        [ p [ ] [ str "The body" ]; root ]
                      Quickview.footer [ ]
                        [ Button.button [ Button.OnClick (fun _ -> u.Id |> ToggleAddCreditsForm |> dispatch) ]
                                        [ str "Hide the quickview!" ] ] ]
            ]
    | None -> str ""


let render (state : State) (dispatch : Msg -> unit) =
    let rows = state.Users |> List.map (userRow dispatch)
    let table =
        Table.table [ Table.IsHoverable ]
            [
                thead [ ]
                    [ tr [ ] [
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
        state.Users |> List.tryFind (fun x -> Some x.Id = state.AddCreditsOpenedForm) |> showForm dispatch state
    ]