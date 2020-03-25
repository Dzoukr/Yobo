module Yobo.Client.State

open System
open Domain
open Elmish
open Feliz.Router
open Server
open Yobo.Client
open Yobo.Client.Router

let init () =
    let currentPage = (Router.currentPath() |> Page.parseFromUrlSegments)
    (Model.init currentPage), (currentPage |> UrlChanged |> Cmd.ofMsg)

let private handleUpdate<'subModel,'subCmd> (fn:'subModel -> 'subModel * Cmd<'subCmd>) mapFn (m:Model) =
    let pageModel = m |> Model.getPageModel<'subModel>
    let newSubModel,subCmd = fn pageModel
    (m |> Model.setPageModel newSubModel), (Cmd.map(mapFn) subCmd)

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
            // refresh user first
            model, Cmd.ofMsg <| RefreshUserWithRedirect(p)
        else
            // user is logged, we can continue
            { model with PageWithModel = p |> PageWithModel.create }, getPageInitCommands p
    | RefreshUserWithRedirect p ->                
        { model with IsCheckingUser = true }, Cmd.OfAsync.eitherAsResult (onUserAccountService (fun x -> x.GetUserInfo)) () (fun u -> UserRefreshedWithRedirect(u, p))
    | UserRefreshedWithRedirect(u, p) ->
        match u with
        | Ok usr -> { model with LoggedUser = Some usr; IsCheckingUser = false }, UrlChanged(p) |> Cmd.ofMsg
        | Error _ -> { model with IsCheckingUser = false }, Cmd.ofMsg LoggedOut
    | RefreshUser ->
        { model with IsCheckingUser = true }, Cmd.OfAsync.eitherAsResult (onUserAccountService (fun x -> x.GetUserInfo)) () UserRefreshed
    | UserRefreshed res ->
        match res with
        | Ok usr -> { model with LoggedUser = Some usr; IsCheckingUser = false }, Cmd.none
        | Error _ -> { model with IsCheckingUser = false }, Cmd.ofMsg LoggedOut
    | RefreshToken token -> model, Cmd.OfAsync.eitherAsResult authService.RefreshToken token TokenRefreshed
    | TokenRefreshed res ->
        match res with
        | Ok t ->
            TokenStorage.setToken t
            model, Cmd.none
        | Error _ -> model, Cmd.ofMsg LoggedOut
    | LoggedOut ->
        TokenStorage.removeToken()
        { model with LoggedUser = None }, Router.navigatePage Login
    // auth
    | LoginMsg subMsg -> model |> handleUpdate (Pages.Login.State.update subMsg) LoginMsg
    | RegistrationMsg subMsg -> model |> handleUpdate (Pages.Registration.State.update subMsg) RegistrationMsg
    | AccountActivationMsg subMsg -> model |> handleUpdate (Pages.AccountActivation.State.update subMsg) AccountActivationMsg
    | ForgottenPasswordMsg subMsg -> model |> handleUpdate (Pages.ForgottenPassword.State.update subMsg) ForgottenPasswordMsg
    | ResetPasswordMsg subMsg -> model |> handleUpdate (Pages.ResetPassword.State.update subMsg) ResetPasswordMsg

let subscribe (_:Model) =
    let sub dispatch = 
        let timer = (TimeSpan.FromMinutes 1.).TotalMilliseconds |> int
        
        let handler _ =
            match TokenStorage.tryGetToken() with
            | Some t -> dispatch (RefreshToken t) |> ignore
            | None -> Cmd.none |> ignore
        Browser.Dom.window.setInterval(handler, timer) |> ignore
    Cmd.ofSub sub