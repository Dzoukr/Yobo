module Yobo.Client.Domain

open Yobo.Shared.Domain
open Router
open Yobo.Client.Interfaces

type CurrentPage =
    | Anonymous of AnonymousPage
    | Secured of SecuredPage * Yobo.Shared.UserAccount.Domain.Queries.UserAccount

type Model = {
    SubPageModel : obj
    IsCheckingUser : bool
    CurrentPage : CurrentPage
}

module Model =
    let getPageModel<'a> (m:Model) = m.SubPageModel :?> 'a
    let setPageModel (m:obj) (model:Model) = { model with SubPageModel = m }

type Msg =
    // auth
    | RefreshUser
    | UserRefreshed of ServerResult<Yobo.Shared.UserAccount.Domain.Queries.UserAccount>
    | RefreshUserWithRedirect of SecuredPage
    | UserRefreshedWithRedirect of SecuredPage * ServerResult<Yobo.Shared.UserAccount.Domain.Queries.UserAccount> 
    | RefreshToken of string
    | TokenRefreshed of ServerResult<string>
    | LoggedOut
    // navigation
    | UrlChanged of Page
    // sub pages
    | LoginMsg of Pages.Login.Domain.Msg
    | RegistrationMsg of Pages.Registration.Domain.Msg
    | AccountActivationMsg of Pages.AccountActivation.Domain.Msg
    | ForgottenPasswordMsg of Pages.ForgottenPassword.Domain.Msg
    | ResetPasswordMsg of Pages.ResetPassword.Domain.Msg
    | UsersMsg of Pages.Users.Domain.Msg
    | MyAccountMsg of Pages.MyAccount.Domain.Msg