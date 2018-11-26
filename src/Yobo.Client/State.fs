module Yobo.Client.State

open Yobo.Client.Domain
open Yobo.Client
open Elmish
open Fable.PowerPack.Fetch
open Thoth.Json
open Fable.Import

let urlUpdate (result: Option<Router.Page>) state =
    match result with
    | None ->
        Browser.console.error("Error parsing url: " + Browser.window.location.href)
        state, Router.modifyUrl state.Page
    | Some page ->
        let state = { state with Page = page }
        state, Cmd.none

let private mapUpdate fn1 fn2 (f,s) = (fn1 f), (s |> Cmd.map fn2)

let init result =
    urlUpdate result State.Init
 
let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | LoginMsg m -> Login.State.update m state.LoginState |> mapUpdate (fun s -> { state with LoginState = s }) Msg.LoginMsg
    | RegisterMsg m -> Register.State.update m state.RegisterState |> mapUpdate (fun s -> { state with RegisterState = s }) Msg.RegisterMsg
    | AccountActivationMsg m -> ActivateAccount.State.update m state.AccountActivationState |> mapUpdate (fun s -> { state with AccountActivationState = s }) Msg.AccountActivationMsg 