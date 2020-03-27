module Yobo.Client.State

open System
open Elmish
open Feliz.Router
open Server
open Yobo.Client
open Yobo.Client.Router
open Domain
open Yobo.Client.Interfaces
open Fable.Core.JsInterop

module CurrentPage =
    let getInitSubPageModel = function
        | CurrentPage.Anonymous p ->
            match p with
            | AccountActivation i -> i |> Pages.AccountActivation.Domain.Model.init |> box
            | ResetPassword i -> i |> Pages.ResetPassword.Domain.Model.init |> box
            | Login -> Pages.Login.Domain.Model.init |> box
            | Registration -> Pages.Registration.Domain.Model.init |> box
            | ForgottenPassword -> Pages.ForgottenPassword.Domain.Model.init |> box
        | CurrentPage.Secured (p,u) ->
            match p with
            | Calendar -> null
            | Lessons -> null
            | Users -> Pages.Users.Domain.Model.init |> box
            | MyAccount -> u |> Pages.MyAccount.Domain.Model.init |> box
    let init = CurrentPage.Anonymous Login
    
module Model =
    let init = {
        CurrentPage = CurrentPage.init
        SubPageModel = CurrentPage.init |> CurrentPage.getInitSubPageModel
        IsCheckingUser = false
    }
    
    let navigateToAnonymous (p:AnonymousPage) (m:Model) =
        let newPage = CurrentPage.Anonymous(p)
        let newSubModel = newPage |> CurrentPage.getInitSubPageModel
        { m with CurrentPage = newPage; SubPageModel = newSubModel }
    
    let navigateToSecured user (p:SecuredPage) (m:Model) =
        let newPage = CurrentPage.Secured(p, user)
        let newSubModel = newPage |> CurrentPage.getInitSubPageModel
        { m with CurrentPage = newPage; SubPageModel = newSubModel }
    
    let refreshUser user (m:Model) =
        match m.CurrentPage with
        | CurrentPage.Anonymous _ -> m
        | CurrentPage.Secured(p,_) ->
            let newPage = Secured(p, user)
            let newSubModel =
                // ugly hack because of error FABLE: Cannot type test: interface
                match m.SubPageModel?UpdateUser with
                | null -> m.SubPageModel
                | _ -> user |> (m.SubPageModel :?> IUserAwareModel).UpdateUser |> box
            { m with CurrentPage = newPage; SubPageModel = newSubModel }    

let init () =
    let nextPage = (Router.currentPath() |> Page.parseFromUrlSegments)
    Model.init, (nextPage |> UrlChanged |> Cmd.ofMsg)
    
let private handleUpdate<'subModel,'subCmd> (fn:'subModel -> 'subModel * Cmd<'subCmd>) mapFn (m:Model) =
    let pageModel = m |> Model.getPageModel<'subModel>
    let newSubModel,subCmd = fn pageModel
    (m |> Model.setPageModel newSubModel), (Cmd.map(mapFn) subCmd)

let private getPageInitCommands targetPage =
    match targetPage with
    | Page.Anonymous (AccountActivation _) -> Pages.AccountActivation.Domain.Msg.Activate |> AccountActivationMsg |> Cmd.ofMsg
    | Page.Secured MyAccount -> RefreshUser |> Cmd.ofMsg
    | Page.Secured Users -> Pages.Users.Domain.LoadUsers |> UsersMsg |> Cmd.ofMsg
    | _ -> Cmd.none

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | UrlChanged page ->
        match model.CurrentPage, page with
        | CurrentPage.Secured(_,user), Page.Secured targetPage -> 
            let newModel = model |> Model.navigateToSecured user targetPage
            newModel, (getPageInitCommands page)
        | CurrentPage.Anonymous _, Page.Anonymous targetPage
        | CurrentPage.Secured _, Page.Anonymous targetPage ->
            let newModel = model |> Model.navigateToAnonymous targetPage
            newModel, (getPageInitCommands page)
        | CurrentPage.Anonymous _, Page.Secured targetpage ->
            model, Cmd.ofMsg <| RefreshUserWithRedirect(targetpage)
    | RefreshUserWithRedirect p ->                
        { model with IsCheckingUser = true }, Cmd.OfAsync.eitherAsResult (onUserAccountService (fun x -> x.GetUserInfo)) () (fun u -> UserRefreshedWithRedirect(p, u))
    | UserRefreshedWithRedirect(p, u) ->
        let model = { model with IsCheckingUser = false }
        match u with
        | Ok usr -> model |> Model.navigateToSecured usr p, Router.navigatePage (Page.Secured p)
        | Error _ -> { model with IsCheckingUser = false }, Cmd.ofMsg LoggedOut
    | RefreshUser ->
        model, Cmd.OfAsync.eitherAsResult (onUserAccountService (fun x -> x.GetUserInfo)) () UserRefreshed
    | UserRefreshed res ->
        match res with
        | Ok usr -> model |> Model.refreshUser usr, Cmd.none
        | Error _ -> model, Cmd.ofMsg LoggedOut        
    | RefreshToken token -> model, Cmd.OfAsync.eitherAsResult authService.RefreshToken token TokenRefreshed
    | TokenRefreshed res ->
        match res with
        | Ok t ->
            TokenStorage.setToken t
            model, Cmd.none
        | Error _ -> model, Cmd.ofMsg LoggedOut
    | LoggedOut ->
        TokenStorage.removeToken()
        model, Router.navigatePage (Page.Anonymous Login)
    // auth
    | LoginMsg subMsg -> model |> handleUpdate (Pages.Login.State.update subMsg) LoginMsg
    | RegistrationMsg subMsg -> model |> handleUpdate (Pages.Registration.State.update subMsg) RegistrationMsg
    | AccountActivationMsg subMsg -> model |> handleUpdate (Pages.AccountActivation.State.update subMsg) AccountActivationMsg
    | ForgottenPasswordMsg subMsg -> model |> handleUpdate (Pages.ForgottenPassword.State.update subMsg) ForgottenPasswordMsg
    | ResetPasswordMsg subMsg -> model |> handleUpdate (Pages.ResetPassword.State.update subMsg) ResetPasswordMsg
    | MyAccountMsg subMsg -> model |> handleUpdate (Pages.MyAccount.State.update subMsg) MyAccountMsg
    | UsersMsg subMsg -> model |> handleUpdate (Pages.Users.State.update subMsg) UsersMsg

let subscribe (_:Model) =
    let sub dispatch = 
        let timer = (TimeSpan.FromMinutes 1.).TotalMilliseconds |> int
        
        let handler _ =
            match TokenStorage.tryGetToken() with
            | Some t -> dispatch (RefreshToken t) |> ignore
            | None -> Cmd.none |> ignore
        Browser.Dom.window.setInterval(handler, timer) |> ignore
    Cmd.ofSub sub