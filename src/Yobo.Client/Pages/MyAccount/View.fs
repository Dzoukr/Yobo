module Yobo.Client.Pages.MyAccount.View

open Domain
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Yobo.Client
open Yobo.Client.SharedView
open Yobo.Shared.DateTime

let view (props:{| user : Yobo.Shared.Core.UserAccount.Domain.Queries.UserAccount |}) = React.functionComponent(fun () ->
    let model, dispatch = React.useElmish(State.init props.user, State.update, [| |])
    let msg =
        let exp = model.LoggedUser.CreditsExpiration |> Option.map (DateTimeOffset.toCzDate) |> Option.defaultValue ""
        if model.LoggedUser.Credits > 0 then
            sprintf "Na účtu máte <strong>%i</strong> kreditů. Platnost kreditů do <strong>%s</strong>" model.LoggedUser.Credits exp
        else "Na účtu nemáte žádné kredity."
    
    let lessonRows =
        match model.Lessons with
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
            color.isWarning
            prop.children [
                Html.faIcon "fas fa-info-circle"
                Html.span [ prop.dangerouslySetInnerHTML msg ]
            ]
        ]
        Bulma.title.h1 "Rezervované lekce"
        Bulma.table [
            table.isFullWidth
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
)