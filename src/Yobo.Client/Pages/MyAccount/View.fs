module Yobo.Client.Pages.MyAccount.View

open Domain
open Feliz
open Fable.React
open Feliz.Bulma
open Fable.React.Props
open System
open Yobo.Shared
open Yobo.Client
open Yobo.Client.SharedView
open Yobo.Shared.Errors
open Yobo.Shared.DateTime

let view (model:Model) (dispatch:Msg -> unit) =
    let msg =
        let exp = model.LoggedUser.CreditsExpiration |> Option.map (DateTimeOffset.toCzDate) |> Option.defaultValue ""
        if model.LoggedUser.Credits > 0 then
            sprintf "Na účtu máte <strong>%i</strong> kreditů. Platnost kreditů do <strong>%s</strong>" model.LoggedUser.Credits exp
        else "Na účtu nemáte žádné kredity."
    
    let lessonRows =
        match model |> Model.sharedLessons with
        | [] -> [ Html.tr [ prop.colSpan 5; prop.children [ Html.td "Nemáte zarezervovány žádné lekce" ] ] ]
        | rows ->
            rows
            |> List.map (fun r ->
                Html.tr [
                    Html.td r.Name
                    Html.td (r.StartDate |> DateTimeOffset.toCzDate)
                    Html.td (r.StartDate |> DateTimeOffset.toCzTime)
                    Html.td (r.EndDate |> DateTimeOffset.toCzTime)
                    Html.td (r.Payment |> SharedView.Queries.paymentToText)
                ]
            )
            
    Html.div [
        Bulma.notification [
            notification.isWarning
            prop.children [
                Html.faIcon "fas fa-info-circle"
                Html.span [ prop.dangerouslySetInnerHTML msg ]
            ]
        ]
        Bulma.title "Rezervované lekce"
        Bulma.table [
            table.isFullwidth
            table.isStriped
            prop.children [
                Html.thead [
                    Html.tr [
                        Html.td "Lekce"
                        Html.td "Datum"
                        Html.td "Začátek"
                        Html.td "Konec"
                        Html.td "Platba"
                    ]
                ]
                Html.tbody lessonRows
            ]
        ]
    ] 