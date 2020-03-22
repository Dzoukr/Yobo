module Yobo.Client.State

open Domain
open Elmish
open Feliz.Router
open Server

let init () = Model.init, Cmd.none

let private upTo model toState toMsg (m,cmd) =
    { model with CurrentPage = toState(m) }, Cmd.map(toMsg) cmd

let private isSecured page =
    match page with
    | Page.Login _
    | Page.Registration _
    | Page.AccountActivation _
    | Page.ForgottenPassword _
    | Page.ResetPassword _ -> false
    | _ -> true

let private getPageInitCommands targetPage =
    match targetPage with
    | Page.AccountActivation _ -> Pages.AccountActivation.Domain.Msg.Activate |> AccountActivationMsg |> Cmd.ofMsg
    | _ -> Cmd.none

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match model.CurrentPage, msg with
    | _, UrlChanged p ->
        if isSecured p && model.LoggedUser.IsNone then
            model, Cmd.ofMsg (RetrieveLoggedUserAndRedirect(p))
        else            
            { model with CurrentPage = p }, getPageInitCommands p
    | _, RetrieveLoggedUserAndRedirect p ->                
        let x = (onUserAccountService (fun x -> x.GetUserInfo))
        { model with IsCheckingUser = true }, Cmd.OfAsync.perform x () (fun u -> LoggedUserRetrieved(u, p)) // eitherResult userAccountService.GetUserInfo (Server.SecuredParam.create ()) (fun u -> LoggedUserRetrieved(u, p))
    | _, LoggedUserRetrieved(usr, p) ->
        { model with LoggedUser = Some usr; IsCheckingUser = false }, UrlChanged(p) |> Cmd.ofMsg
//        match u with
//        | Ok usr -> { model with LoggedUser = Some usr; IsCheckingUser = false }, UrlChanged(p) |> Cmd.ofMsg
//        | Error e ->
//            { model with IsCheckingUser = false },
//                [ SharedView.ServerResponseViews.showErrorToast e; Router.navigate Paths.Login ] |> Cmd.batch 
    // auth
    | Login m, LoginMsg subMsg -> Pages.Login.State.update subMsg m |> upTo model Login LoginMsg 
    | Registration m, RegistrationMsg subMsg -> Pages.Registration.State.update subMsg m |> upTo model Registration RegistrationMsg
    | AccountActivation m, AccountActivationMsg subMsg -> Pages.AccountActivation.State.update subMsg m |> upTo model AccountActivation AccountActivationMsg
    | ForgottenPassword m, ForgottenPasswordMsg subMsg -> Pages.ForgottenPassword.State.update subMsg m |> upTo model ForgottenPassword ForgottenPasswordMsg
    | ResetPassword m, ResetPasswordMsg subMsg -> Pages.ResetPassword.State.update subMsg m |> upTo model ResetPassword ResetPasswordMsg
