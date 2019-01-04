module Yobo.Client.View

open Yobo.Client.Domain

let render (state : State) (dispatch : Msg -> unit) =
    match state.Page with
    | Router.Page.Public pub ->
        match pub with
        | Router.PublicPage.Login -> Login.View.render state.LoginState (LoginMsg >> dispatch)
        | Router.PublicPage.Registration -> Registration.View.render state.RegistrationState (RegistrationMsg >> dispatch)
        | Router.PublicPage.ForgottenPassword -> Registration.View.render state.RegistrationState (RegistrationMsg >> dispatch)
        | Router.PublicPage.AccountActivation _ -> AccountActivation.View.render state.AccountActivationState (AccountActivationMsg >> dispatch)