module Yobo.Client.State

open Domain
open Elmish
open Feliz.Router
open Server
open Yobo.Client.Router

let init () =
    let currentPage = (Router.currentPath() |> Page.parseFromUrlSegments)
    (Model.init currentPage), (currentPage |> UrlChanged |> Cmd.ofMsg)

let private setPageModel model toMsg (m,cmd) =
    { model with PageWithModel = model.PageWithModel |> PageWithModel.setModel m }, Cmd.map(toMsg) cmd

let private isSecured page =
    match page with
    | Page.Login
    | Page.Registration
    | Page.AccountActivation _
    | Page.ForgottenPassword _
    | Page.ResetPassword _ -> false
    | _ -> true

let private getPageInitCommands targetPage =
    match targetPage with
    | Page.AccountActivation _ -> Pages.AccountActivation.Domain.Msg.Activate |> AccountActivationMsg |> Cmd.ofMsg
    | _ -> Cmd.none

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | UrlChanged p ->
        if isSecured p && model.LoggedUser.IsNone then
            model, Cmd.ofMsg (RetrieveLoggedUserAndRedirect(p))
        else            
            { model with PageWithModel = p |> PageWithModel.create }, getPageInitCommands p
    | RetrieveLoggedUserAndRedirect p ->                
        { model with IsCheckingUser = true }, Cmd.OfAsync.eitherAsResult (onUserAccountService (fun x -> x.GetUserInfo)) () (fun u -> LoggedUserRetrieved(u, p))
    | LoggedUserRetrieved(u, p) ->
        match u with
        | Ok usr -> { model with LoggedUser = Some usr; IsCheckingUser = false }, UrlChanged(p) |> Cmd.ofMsg
        | Error _ ->
            TokenStorage.removeToken()
            { model with IsCheckingUser = false }, Router.navigatePage Login 
    // auth
    | LoginMsg subMsg ->
        model |> Model.getPageModel<Pages.Login.Domain.Model> |> Pages.Login.State.update subMsg |> setPageModel model LoginMsg
    | RegistrationMsg subMsg ->
        model |> Model.getPageModel<Pages.Registration.Domain.Model> |> Pages.Registration.State.update subMsg |> setPageModel model RegistrationMsg
    | AccountActivationMsg subMsg ->
        model |> Model.getPageModel<Pages.AccountActivation.Domain.Model> |> Pages.AccountActivation.State.update subMsg |> setPageModel model AccountActivationMsg
    | ForgottenPasswordMsg subMsg -> 
        model |> Model.getPageModel<Pages.ForgottenPassword.Domain.Model> |> Pages.ForgottenPassword.State.update subMsg |> setPageModel model ForgottenPasswordMsg
    | ResetPasswordMsg subMsg ->
        model |> Model.getPageModel<Pages.ResetPassword.Domain.Model> |> Pages.ResetPassword.State.update subMsg |> setPageModel model ResetPasswordMsg
