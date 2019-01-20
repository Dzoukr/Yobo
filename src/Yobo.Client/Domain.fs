module Yobo.Client.Domain

open Yobo.Shared.Communication

type AuthPage =
    | Login of Auth.Login.Domain.State
    | Registration of Auth.Registration.Domain.State
    | AccountActivation of Auth.AccountActivation.Domain.State

type AdminPage =
    | Users of Admin.Users.Domain.State
    | Lessons of Admin.Lessons.Domain.State

type Page =
    | Auth of AuthPage
    | Admin of AdminPage
    | Calendar of Calendar.Domain.State

type State = {
    Page : Page
    Route : string
    LoggedUser : Yobo.Shared.Domain.User option
}
with
    static member Init = {
        Page = AdminPage.Users(Admin.Users.Domain.State.Init) |> Page.Admin
        LoggedUser = None
        Route = ""
    }

type AuthMsg =
    | LoginMsg of Auth.Login.Domain.Msg
    | RegistrationMsg of Auth.Registration.Domain.Msg
    | AccountActivationMsg of Auth.AccountActivation.Domain.Msg

type AdminMsg =
    | UsersMsg of Admin.Users.Domain.Msg
    | LessonsMsg of Admin.Lessons.Domain.Msg

type Msg =
    | AuthMsg of AuthMsg
    | AdminMsg of AdminMsg
    | CalendarMsg of Calendar.Domain.Msg
    | ReloadUser
    | UserByTokenLoaded of Result<Yobo.Shared.Domain.User,ServerError>
    | RefreshToken of string
    | TokenRefreshed of Result<string, ServerError>
    | LoggedOut