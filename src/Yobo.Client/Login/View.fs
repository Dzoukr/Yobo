module Yobo.Client.Login.View

open Yobo.Client.Login
open Yobo.Client.Login.Domain

let render (state : State) (dispatch : Msg -> unit) =
    match state.Page with
    | Router.Page.SignIn -> SignIn.View.render state.SignInState (LoginMsg >> dispatch)
    | Router.Page.Register -> Register.View.render state.RegisterState (RegisterMsg >> dispatch)