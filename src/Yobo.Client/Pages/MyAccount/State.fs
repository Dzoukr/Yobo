module Yobo.Client.Pages.MyAccount.State

open Yobo.Client.Router
open Domain
open Elmish
open Fable.Core
open Feliz.Router
open Yobo.Client
open Yobo.Shared.Auth.Validation
open Yobo.Client.Server
open Yobo.Client.SharedView
open Yobo.Shared.Auth.Communication
open Yobo.Client.Forms

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | LoadMyLessons ->
        model, Cmd.none// Cmd.OfAsync.eitherAsResult (onUserAccountService (fun x -> x.GetUserInfo)) ()
    
    