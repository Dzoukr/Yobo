module Yobo.Client.Domain

open Yobo.Client

type State = { 
    Page : Router.Page
    LoginState : Login.Domain.State
    RegisterState : Register.Domain.State
    AccountActivationState : ActivateAccount.Domain.State
}
with
    static member Init = {
        Page = Router.Page.Login
        LoginState = Login.Domain.State.Init 
        RegisterState = Register.Domain.State.Init
        AccountActivationState = ActivateAccount.Domain.State.Init
    }

type Msg =
    | LoginMsg of Login.Domain.Msg
    | RegisterMsg of Register.Domain.Msg
    | AccountActivationMsg of ActivateAccount.Domain.Msg