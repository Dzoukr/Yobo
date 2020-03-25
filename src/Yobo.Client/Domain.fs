module Yobo.Client.Domain

open Yobo.Shared.Domain
open Router

let private initPageModel = function
    | Page.AccountActivation i -> box (Pages.AccountActivation.Domain.Model.init i)
    | Page.ResetPassword i -> box (Pages.ResetPassword.Domain.Model.init i)
    | Page.Login -> box Pages.Login.Domain.Model.init
    | Page.Registration -> box Pages.Registration.Domain.Model.init
    | Page.ForgottenPassword -> box Pages.ForgottenPassword.Domain.Model.init
    | Page.Calendar -> null // TODO

type PageWithModel = {
    Page : Page
    Model : obj
}

module PageWithModel =
    let create p = { Page = p; Model = p |> initPageModel }

type Model = {
    PageWithModel : PageWithModel
    LoggedUser : Yobo.Shared.UserAccount.Domain.Queries.UserAccount option
    IsCheckingUser : bool
}

module Model =
    let init (p:Page) = {
        PageWithModel = PageWithModel.create p
        LoggedUser = None
        IsCheckingUser = false
    }

    let getPageModel<'a> (m:Model) = m.PageWithModel.Model :?> 'a
    let setPageModel (m:obj) (model:Model) = { model with PageWithModel = { model.PageWithModel with Model = m } }

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