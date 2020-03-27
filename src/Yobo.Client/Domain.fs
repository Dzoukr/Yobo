module Yobo.Client.Domain

open Yobo.Shared.Domain
open Router

type CurrentPage =
    | Anonymous of AnonymousPage
    | Secured of SecuredPage * Yobo.Shared.UserAccount.Domain.Queries.UserAccount

module CurrentPage =
    let getInitSubPageModel = function
        | Anonymous p ->
            match p with
            | AccountActivation i -> i |> Pages.AccountActivation.Domain.Model.init |> box
            | ResetPassword i -> i |> Pages.ResetPassword.Domain.Model.init |> box
            | Login -> Pages.Login.Domain.Model.init |> box
            | Registration -> Pages.Registration.Domain.Model.init |> box
            | ForgottenPassword -> Pages.ForgottenPassword.Domain.Model.init |> box
        | Secured (p,u) ->
            match p with
            | Calendar -> null
            | Lessons -> null
            | Users -> null
            | MyAccount -> u |> Pages.MyAccount.Domain.Model.init |> box
    let init = Anonymous Login

type Model = {
    SubPageModel : obj
    IsCheckingUser : bool
    CurrentPage : CurrentPage
}

module Model =
    let init = {
        CurrentPage = CurrentPage.init
        SubPageModel = CurrentPage.init |> CurrentPage.getInitSubPageModel
        IsCheckingUser = false
    }
    
    let navigateToAnonymous (p:AnonymousPage) (m:Model) =
        let newPage = Anonymous(p)
        let newSubModel = newPage |> CurrentPage.getInitSubPageModel
        { m with CurrentPage = newPage; SubPageModel = newSubModel }
    
    let navigateToSecured user (p:SecuredPage) (m:Model) =
        let newPage = Secured(p, user)
        let newSubModel = newPage |> CurrentPage.getInitSubPageModel
        { m with CurrentPage = newPage; SubPageModel = newSubModel }
    
    let getPageModel<'a> (m:Model) = m.SubPageModel :?> 'a
    let setPageModel (m:obj) (model:Model) = { model with SubPageModel = m }
    
    let refreshUser user (m:Model) =
        match m.CurrentPage with
        | Anonymous _ -> m
        | Secured(p,_) ->
            let newPage = Secured(p, user)
            let newSubModel =
                match p with
                | MyAccount ->
                    let model = m |> getPageModel<Pages.MyAccount.Domain.Model>
                    { model with LoggedUser = user } |> box
                | _ -> newPage |> CurrentPage.getInitSubPageModel
            { m with CurrentPage = newPage; SubPageModel = newSubModel }

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
    | MyAccountMsg of Pages.MyAccount.Domain.Msg