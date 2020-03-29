module Yobo.Client.Pages.MyAccount.View

open Domain
open Feliz
open Feliz.Bulma
open Fable.React
open Fable.React.Props
open System
open Yobo.Shared
open Yobo.Client
open Yobo.Shared.Errors

let view (model:Model) (dispatch:Msg -> unit) =
    Html.text (sprintf "%A" model.LoggedUser) 
    
//    let txt = 
//        match state.LoggedUser with
//        | Some user ->
//            let exp = user.CreditsExpiration |> Option.map (fun x -> x.ToString("dd. MM. yyyy")) |> Option.defaultValue ""
//            if user.Credits > 0 then
//                sprintf " Na účtu máte <strong>%i</strong> kreditů. Platnost kreditů do <strong>%s</strong>" user.Credits exp
//            else " Na účtu nemáte žádné kredity."
//        | None -> ""
//
//    let rows =
//        match state.Lessons with
//        | [] -> [ td [ ColSpan 5] [ str "Nemáte zarezervovány žádné lekce"] ]
//        | rows ->
//            rows
//            |> List.map (fun r -> 
//                let payment = if r.CreditsUsed then "Kredity" else "Hotově"
//                tr [] [
//                    td [] [ str r.Name ]
//                    td [] [ str <| r.StartDate.ToString("dd. MM. yyyy") ]
//                    td [] [ str <| r.StartDate.ToString("HH:mm") ]
//                    td [] [ str <| r.EndDate.ToString("HH:mm") ]
//                    td [] [ str payment ]
//                ]
//            )
//
//    div [] [
//        Notification.notification [ Notification.Option.Color IsWarning ] [
//            i [ Class "fas fa-info-circle"] [ ]
//            span [ DangerouslySetInnerHTML { __html = txt } ] [ ]
//        ]
//        
//        h1 [ Class "title"] [ str "Rezervované lekce" ]
//        Table.table [ Table.TableOption.IsFullWidth; Table.TableOption.IsStriped ] [
//            thead [] [
//                tr [] [
//                    th [] [ str "Lekce"]
//                    th [] [ str "Datum"]
//                    th [] [ str "Začátek"]
//                    th [] [ str "Konec"]
//                    th [] [ str "Platba"]
//                ]
//            ]
//            tbody [] rows
//        ]
//    ]