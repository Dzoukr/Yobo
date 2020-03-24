module Yobo.Client.Domain

open Yobo.Shared.Domain

type Page =
    | Calendar
    // auth
    | Login of Pages.Login.Domain.Model
    | Registration of Pages.Registration.Domain.Model
    | AccountActivation of Pages.AccountActivation.Domain.Model
    | ForgottenPassword of Pages.ForgottenPassword.Domain.Model
    | ResetPassword of Pages.ResetPassword.Domain.Model

[<RequireQualifiedAccess>]    
module Page =
    open Feliz.Router
    
    let parseFromUrlSegments = function
        | [ Paths.Login ] -> Login (Pages.Login.Domain.Model.init)
        | [ Paths.Registration ] -> Registration Pages.Registration.Domain.Model.init
        | [ Paths.Calendar ] -> Calendar
        | [ Yobo.Shared.ClientPaths.AccountActivation; Route.Guid id ] -> AccountActivation (Pages.AccountActivation.Domain.Model.init id)
        | [ Paths.ForgottenPassword ] -> ForgottenPassword (Pages.ForgottenPassword.Domain.Model.init)
        | [ Yobo.Shared.ClientPaths.ResetPassword; Route.Guid id ] -> ResetPassword (Pages.ResetPassword.Domain.Model.init id)
        | _ -> Calendar
        
//    let getPath = function
//        | Login _ -> Paths.Login
//        

type Model = {
    CurrentPage : Page
    LoggedUser : Yobo.Shared.UserAccount.Domain.Queries.UserAccount option
    IsCheckingUser : bool
}

module Model =
    let init page = {
        CurrentPage = page
        LoggedUser = None
        IsCheckingUser = false
    }

type Msg =
    | RetrieveLoggedUserAndRedirect of Page
    | LoggedUserRetrieved of ServerResult<Yobo.Shared.UserAccount.Domain.Queries.UserAccount> * Page
    // navigation
    | UrlChanged of Page
    // auth
    | LoginMsg of Pages.Login.Domain.Msg
    | RegistrationMsg of Pages.Registration.Domain.Msg
    | AccountActivationMsg of Pages.AccountActivation.Domain.Msg
    | ForgottenPasswordMsg of Pages.ForgottenPassword.Domain.Msg
    | ResetPasswordMsg of Pages.ResetPassword.Domain.Msg