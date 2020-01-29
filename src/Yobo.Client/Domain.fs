module Yobo.Client.Domain

type Page =
    | Calendar
    // auth
    | Login of Pages.Login.Domain.Model
    | Registration of Pages.Registration.Domain.Model
    | AccountActivation of Pages.AccountActivation.Domain.Model
    | ForgottenPassword of Pages.ForgottenPassword.Domain.Model
    | ResetPassword of Pages.ResetPassword.Domain.Model

type Model = {
    CurrentPage : Page
    LoggedUser : string option
    IsCheckingUser : bool
}

module Model =
    let init = {
        CurrentPage = Calendar
        LoggedUser = None
        IsCheckingUser = false
    }

type Msg =
    | RetrieveLoggedUserAndRedirect of Page
    | LoggedUserRetrieved of string * Page
    // navigation
    | UrlChanged of Page
    // auth
    | LoginMsg of Pages.Login.Domain.Msg
    | RegistrationMsg of Pages.Registration.Domain.Msg
    | AccountActivationMsg of Pages.AccountActivation.Domain.Msg
    | ForgottenPasswordMsg of Pages.ForgottenPassword.Domain.Msg
    | ResetPasswordMsg of Pages.ResetPassword.Domain.Msg