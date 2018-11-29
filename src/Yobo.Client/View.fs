module Yobo.Client.View

open Yobo.Client.Domain

let render (state : State) (dispatch : Msg -> unit) =
    match state.Page with
    | Router.Page.Login -> Login.View.render state.LoginState (LoginMsg >> dispatch)
    | Router.Page.Registration -> Registration.View.render state.RegistrationState (RegistrationMsg >> dispatch)
    | Router.Page.ForgottenPassword -> Registration.View.render state.RegistrationState (RegistrationMsg >> dispatch)
    | Router.Page.AccountActivation _ -> AccountActivation.View.render state.AccountActivationState (AccountActivationMsg >> dispatch)