module Yobo.Client.Login.Domain

open Yobo.Client.Login

type State = { 
    Page : Router.Page
    SignInState : SignIn.Domain.State
    RegisterState : Register.Domain.State
}
with
    static member Init = {
        Page = Router.Page.SignIn
        SignInState = SignIn.Domain.State.Init 
        RegisterState = Register.Domain.State.Init 
    }

type Msg =
    | LoginMsg of SignIn.Domain.Msg
    | RegisterMsg of Register.Domain.Msg