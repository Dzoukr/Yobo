module Yobo.Client.Pages.MyAccount.State

open Domain
open Elmish
open Yobo.Client.Server
open Yobo.Client.SharedView

let init user =
    {
        LoggedUser = user
        Lessons = []
    }, Cmd.ofMsg LoadMyLessons

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | LoadMyLessons -> model, Cmd.OfAsync.eitherAsResult (onUserAccountService (fun x -> x.GetMyLessons)) () MyLessonsLoaded
    | MyLessonsLoaded res ->
        match res with
        | Ok lsn -> { model with Lessons = lsn |> List.sortBy (fun x -> x.StartDate) }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast
    
    