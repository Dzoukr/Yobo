module Yobo.Client.State

open Elmish
open Fable.Import
open Yobo.Client.Domain
open Http
open System

let urlUpdate (result: Option<Router.Page>) state =
    match result with
    | None ->
        state, Router.newUrl(Router.Page.DefaultPage)
    | Some page ->
        let state = { state with Page = page }
        match page with
        | Router.Page.Auth(Router.AuthPage.AccountActivation id) ->
            state,
                id
                |> Auth.AccountActivation.Domain.Msg.Activate
                |> AuthMsg.AccountActivationMsg
                |> AuthMsg
                |> Cmd.ofMsg
        | Router.Page.Admin _ ->
            state, match state.LoggedUser, TokenStorage.tryGetToken() with
                    | Some _, Some _ -> Cmd.none
                    | None, Some t -> Cmd.ofMsg (Msg.LoadUserByToken t)
                    | _, None -> Router.newUrl(Router.Page.Auth(Router.AuthPage.Logout))
        | Router.Page.Auth(Router.AuthPage.Logout) ->
            TokenStorage.removeToken()
            { state with LoggedUser = None}, Router.newUrl(Router.Page.Auth(Router.AuthPage.Login))
        | _ -> state, Cmd.none

let private mapUpdate fn1 fn2 (f,s) = (fn1 f), (s |> Cmd.map fn2)

let init result =
    urlUpdate result State.Init
 
let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | AuthMsg m ->
        match m with
        | LoginMsg m ->
            Auth.Login.State.update m state.Auth.Login
            |> mapUpdate (fun s -> { state with Auth = { state.Auth with Login = s } }) (LoginMsg >> Msg.AuthMsg)
        | RegistrationMsg m ->
            Auth.Registration.State.update m state.Auth.Registration
            |> mapUpdate (fun s -> { state with Auth = { state.Auth with Registration = s } }) (RegistrationMsg >> Msg.AuthMsg)
        | AccountActivationMsg m ->
            Auth.AccountActivation.State.update m state.Auth.AccountActivation
            |> mapUpdate (fun s -> { state with Auth = { state.Auth with AccountActivation = s } }) (AccountActivationMsg >> Msg.AuthMsg)
    | LoadUserByToken t -> state, (t |> Cmd.ofAsyncResult authAPI.GetUserByToken UserByTokenLoaded)
    | UserByTokenLoaded res ->
        match res with
        | Ok user -> { state with LoggedUser = Some user }, Cmd.none
        | Error _ -> state, Router.newUrl(Router.Page.Auth(Router.AuthPage.Logout))
    | RefreshToken t -> state, (t |> Cmd.ofAsyncResult authAPI.RefreshToken TokenRefreshed)
    | TokenRefreshed res ->
        match res with
        | Ok t ->
            t |> TokenStorage.setToken
            state, Cmd.none
        | Error _ -> state, Router.newUrl(Router.Page.Auth(Router.AuthPage.Logout))

let subscribe (state:State) =
    let sub dispatch = 
        let timer = (TimeSpan.FromMinutes 1.).TotalMilliseconds |> int
        
        let handler _ =
            match TokenStorage.tryGetToken() with
            | Some t -> dispatch (RefreshToken t) |> ignore
            | None -> Cmd.none |> ignore
        Fable.Import.Browser.window.setInterval(handler, timer) |> ignore
    Cmd.ofSub sub