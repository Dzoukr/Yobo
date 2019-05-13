module Yobo.Client.Domain

open Yobo.Shared.Communication
open System.Collections.Generic
open Router

type States = {
    Login : Auth.Login.Domain.State
    Registration : Auth.Registration.Domain.State
    AccountActivation : Auth.AccountActivation.Domain.State
    ForgottenPassword : Auth.ForgottenPassword.Domain.State
    ResetPassword : Auth.ResetPassword.Domain.State
    Users : Admin.Users.Domain.State
    Lessons : Admin.Lessons.Domain.State
    Calendar : Calendar.Domain.State
    MyLessons : MyLessons.Domain.State
}
with
    static member Init = {
        Login = Auth.Login.Domain.State.Init
        Registration = Auth.Registration.Domain.State.Init
        AccountActivation = Auth.AccountActivation.Domain.State.Init
        ForgottenPassword = Auth.ForgottenPassword.Domain.State.Init
        ResetPassword = Auth.ResetPassword.Domain.State.Init
        Users = Admin.Users.Domain.State.Init
        Lessons = Admin.Lessons.Domain.State.Init
        Calendar = Calendar.Domain.State.Init
        MyLessons = MyLessons.Domain.State.Init
    }

type State = {
    Page : Router.Page
    States : States
    LoggedUser : Yobo.Shared.Domain.User option
    TermsDisplayed : bool
}
with
    static member Init = {
        Page = Page.AdminPage(AdminPage.Users)
        States = States.Init
        LoggedUser = None
        TermsDisplayed = false
    }

type Dictionary<'k,'v> with
    member x.GetState name def =
        if x.ContainsKey(name) then x.[name]
        else
            x.[name] <- def
            def
    member x.SetState name state = x.[name] <- state

type AuthMsg =
    | LoginMsg of Auth.Login.Domain.Msg
    | RegistrationMsg of Auth.Registration.Domain.Msg
    | AccountActivationMsg of Auth.AccountActivation.Domain.Msg
    | ForgottenPasswordMsg of Auth.ForgottenPassword.Domain.Msg
    | ResetPasswordMsg of Auth.ResetPassword.Domain.Msg

type AdminMsg =
    | UsersMsg of Admin.Users.Domain.Msg
    | LessonsMsg of Admin.Lessons.Domain.Msg

type Msg =
    | AuthMsg of AuthMsg
    | AdminMsg of AdminMsg
    | CalendarMsg of Calendar.Domain.Msg
    | MyLessonsMsg of MyLessons.Domain.Msg
    | ReloadUser
    | UserByTokenLoaded of Result<Yobo.Shared.Domain.User,ServerError>
    | RefreshToken of string
    | TokenRefreshed of Result<string, ServerError>
    | ToggleTermsView
    | LoggedOut