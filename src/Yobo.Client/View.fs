module Yobo.Client.View

open Yobo.Client
open Yobo.Client.Domain

let render (state : State) (dispatch : Msg -> unit) =
    match state.Page with
    | Router.Page.Login -> Login.View.render state.LoginState (LoginMsg >> dispatch)
    | Router.Page.Register -> Register.View.render state.RegisterState (RegisterMsg >> dispatch)
    | Router.Page.ForgottenPassword -> Register.View.render state.RegisterState (RegisterMsg >> dispatch)
    | Router.Page.ActivateAccount id -> ActivateAccount.View.render id state.AccountActivationState (AccountActivationMsg >> dispatch)