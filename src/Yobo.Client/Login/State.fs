module Yobo.Client.Login.State

open Yobo.Client.Login.Domain
open Yobo.Client.Login
open Elmish
open Fable.PowerPack.Fetch
open Yobo.Shared
open Thoth.Json
open Fable.Import

//let initialCounter = fetchAs<Counter> "/api/init" (Decode.Auto.generateDecoder<Counter>(true))

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
    | LoginMsg m -> SignIn.State.update m state.SignInState |> mapUpdate (fun s -> { state with SignInState = s }) Msg.LoginMsg
    | RegisterMsg m -> Register.State.update m state.RegisterState |> mapUpdate (fun s -> { state with RegisterState = s }) Msg.RegisterMsg