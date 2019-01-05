module Yobo.Client.View

open Yobo.Client.Domain
open Fulma
open Fable.Helpers.React
open Fable.Helpers.React.Props

let render (state : State) (dispatch : Msg -> unit) =
    match state.Page with
    | Router.Page.Auth auth ->
        match auth with
        | Router.AuthPage.Login -> Auth.Login.View.render state.Auth.Login (LoginMsg >> AuthMsg >> dispatch)
        | Router.AuthPage.Registration -> Auth.Registration.View.render state.Auth.Registration (RegistrationMsg >> AuthMsg >> dispatch)
        | Router.AuthPage.ForgottenPassword -> Auth.Registration.View.render state.Auth.Registration (RegistrationMsg >> AuthMsg >> dispatch)
        | Router.AuthPage.AccountActivation _ -> Auth.AccountActivation.View.render state.Auth.AccountActivation (AccountActivationMsg >> AuthMsg >> dispatch)
        | Router.AuthPage.Logout -> str ""
    | Router.Page.Admin admin ->
        match admin with
        | Router.AdminPage.Users -> div [] [ str ("USEEEEEEEEEEERS pro " + (string state.LoggedUser)) ]
