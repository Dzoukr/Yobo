module Yobo.Client.Domain

type State = { 
    Page : Router.Page
    LoginState : Login.Domain.State
    RegistrationState : Registration.Domain.State
    AccountActivationState : AccountActivation.Domain.State
}
with
    static member Init = {
        Page = Router.Page.Login
        LoginState = Login.Domain.State.Init 
        RegistrationState = Registration.Domain.State.Init
        AccountActivationState = AccountActivation.Domain.State.Init
    }

type Msg =
    | LoginMsg of Login.Domain.Msg
    | RegistrationMsg of Registration.Domain.Msg
    | AccountActivationMsg of AccountActivation.Domain.Msg