module Yobo.Client.Auth.Login.State

open Elmish
open Yobo.Client.Auth.Login.Domain
open Yobo.Client.Http
open Yobo.Client
open Elmish.Browser.Navigation

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | Login -> { state with IsLogging = true }, (state.Login |> Cmd.ofAsyncResult authAPI.GetToken LoggedIn)
    | ResendActivation id -> state, (id |> Cmd.ofAsyncResult authAPI.ResendActivation ActivationResent)
    | LoggedIn res ->
        match res with
        | Ok token ->
            token |> TokenStorage.setToken
            { state with
                IsLogging = false
                LoginResult = None
                ResendActivationResult = None
                Login = Yobo.Shared.Auth.Domain.Login.Init
            }, Navigation.newUrl Router.Routes.defaultPage
        | Error _ -> { state with IsLogging = false; LoginResult = Some res; ResendActivationResult = None }, Cmd.none
    | ActivationResent res -> { state with ResendActivationResult = Some res }, Cmd.none
    | ChangeEmail v -> { state with Login = { state.Login with Email = v }}, Cmd.none
    | ChangePassword v -> { state with Login = { state.Login with Password = v }}, Cmd.none