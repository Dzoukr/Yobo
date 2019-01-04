module Yobo.Client.Domain

open Yobo.Shared.Communication

type State = { 
    Page : Router.Page
    LoggedUser : Yobo.Shared.Domain.User option

    // pages state
    LoginState : Login.Domain.State
    RegistrationState : Registration.Domain.State
    AccountActivationState : AccountActivation.Domain.State

}
with
    static member Init = {
        Page = Router.Page.Public(Router.PublicPage.Login)
        LoggedUser = None
        LoginState = Login.Domain.State.Init 
        RegistrationState = Registration.Domain.State.Init
        AccountActivationState = AccountActivation.Domain.State.Init
    }

type Msg =
    | LoginMsg of Login.Domain.Msg
    | RegistrationMsg of Registration.Domain.Msg
    | AccountActivationMsg of AccountActivation.Domain.Msg
    | GetUser of string
    | GetUserDone of Result<Yobo.Shared.Domain.User,ServerError>