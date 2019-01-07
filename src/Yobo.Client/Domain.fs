module Yobo.Client.Domain

open Yobo.Shared.Communication


type AuthState = {
    Login : Auth.Login.Domain.State
    Registration : Auth.Registration.Domain.State
    AccountActivation : Auth.AccountActivation.Domain.State
}
with
    static member Init = {
        Login = Auth.Login.Domain.State.Init
        Registration = Auth.Registration.Domain.State.Init
        AccountActivation = Auth.AccountActivation.Domain.State.Init
    }

type State = { 
    Page : Router.Page
    LoggedUser : Yobo.Shared.Auth.Domain.LoggedUser option

    // pages state
    Auth : AuthState
    Admin : Admin.Domain.State
}
with
    static member Init = {
        Page = Router.Page.DefaultPage
        LoggedUser = None
        Auth = AuthState.Init
        Admin = Admin.Domain.State.Init
    }

type AuthMsg =
    | LoginMsg of Auth.Login.Domain.Msg
    | RegistrationMsg of Auth.Registration.Domain.Msg
    | AccountActivationMsg of Auth.AccountActivation.Domain.Msg

type Msg =
    | AuthMsg of AuthMsg
    | AdminMsg of Admin.Domain.Msg
    | LoadUserByToken of string
    | UserByTokenLoaded of Result<Yobo.Shared.Auth.Domain.LoggedUser,ServerError>
    | RefreshToken of string
    | TokenRefreshed of Result<string, ServerError>