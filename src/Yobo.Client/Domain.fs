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

type AdminState = {
    Users : Admin.Users.Domain.State
}
with
    static member Init = {
        Users = Admin.Users.Domain.State.Init
    }

type State = { 
    Page : Router.Page
    LoggedUser : Yobo.Shared.Auth.Domain.LoggedUser option

    // pages state
    Auth : AuthState
    Admin : AdminState
}
with
    static member Init = {
        Page = Router.Page.DefaultPage
        LoggedUser = None
        Auth = AuthState.Init
        Admin = AdminState.Init
    }

type AuthMsg =
    | LoginMsg of Auth.Login.Domain.Msg
    | RegistrationMsg of Auth.Registration.Domain.Msg
    | AccountActivationMsg of Auth.AccountActivation.Domain.Msg

type AdminMsg =
    | UsersMsg of Admin.Users.Domain.Msg

type Msg =
    | AuthMsg of AuthMsg
    | AdminMsg of AdminMsg
    | LoadUserByToken of string
    | UserByTokenLoaded of Result<Yobo.Shared.Auth.Domain.LoggedUser,ServerError>
    | RefreshToken of string
    | TokenRefreshed of Result<string, ServerError>