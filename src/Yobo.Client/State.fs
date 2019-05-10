module Yobo.Client.State

open Elmish
open Fable.Import
open Yobo.Client.Domain
open Http
open System
open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation

let private removeSlash (s:string) = s.TrimStart([|'/'|])
let inline private s p = p |> removeSlash |> s

let pageParser: Parser<Page -> Page, Page> =
    oneOf [
        map (Auth(ForgottenPassword(Auth.ForgottenPassword.Domain.State.Init))) (s Router.Routes.forgottenPassword)
        map (Auth(Login(Auth.Login.Domain.State.Init))) (s Router.Routes.login)
        map (Auth(Registration(Auth.Registration.Domain.State.Init))) (s Router.Routes.registration)
        map ((fun (x:string) -> Guid(x)) >> Auth.AccountActivation.Domain.State.Init >> AccountActivation >> Auth) (s Router.Routes.accountActivation </> str)
        map ((fun (x:string) -> Guid(x)) >> Auth.ResetPassword.Domain.State.Init >> ResetPassword >> Auth) (s Router.Routes.resetPassword </> str)
        map (Admin(Users(Admin.Users.Domain.State.Init))) (s Router.Routes.users)
        map (Admin(Lessons(Admin.Lessons.Domain.State.Init))) (s Router.Routes.lessons)
        map (Calendar(Calendar.Domain.State.Init)) (s Router.Routes.calendar)
        map (MyLessons(MyLessons.Domain.State.Init)) (s Router.Routes.mylessons)
    ]

let private withCheckingLogin (state:State) cmd =
    let requiresLogin =
        match state.Page with
        | Auth _ -> false
        | _ -> true

    if requiresLogin then
        match state.LoggedUser, TokenStorage.tryGetToken() with
        | None, Some _ -> [ Cmd.ofMsg Msg.ReloadUser; cmd ] |> Cmd.batch
        | Some _, Some _ -> cmd
        | _ -> LoggedOut |> Cmd.ofMsg
    else cmd

let private withRoute (state:State) = { state with Route = Fable.Import.Browser.location.pathname }

let urlUpdate (result: Option<Page>) state =
    match result with
    | None ->
        state |> withRoute, Navigation.newUrl Router.Routes.defaultPage
    | Some page ->
        let state = { state with Page = page }
        let cmd =
            match page with
            | Auth pg ->
                match pg with
                | AccountActivation s ->
                    s.Id
                    |> Auth.AccountActivation.Domain.Msg.Activate
                    |> AuthMsg.AccountActivationMsg
                    |> AuthMsg
                    |> Cmd.ofMsg
                | _ -> Cmd.none
            | Admin pg ->
                match pg with
                | Users _ -> Admin.Users.Domain.Msg.Init |> UsersMsg |> AdminMsg |> Cmd.ofMsg
                | Lessons _ -> Admin.Lessons.Domain.Msg.Init |> LessonsMsg |> AdminMsg |> Cmd.ofMsg
            | Calendar _ -> Calendar.Domain.Msg.Init |> CalendarMsg |> Cmd.ofMsg
            | MyLessons _ -> MyLessons.Domain.Msg.Init |> MyLessonsMsg |> Cmd.ofMsg
        (state |> withRoute), (withCheckingLogin state cmd)

let init result =
    urlUpdate result State.Init
 
let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    let map stateMap cmdMap (subState,subCmd) =
        { state with Page = (subState |> stateMap) }, (subCmd |> Cmd.map cmdMap)

    let mapWithReloadUser stateMap cmdMap (subState,subCmd,reloadUser) =
        let reloadCmd = if reloadUser then ReloadUser |> Cmd.ofMsg else Cmd.none
        { state with Page = (subState |> stateMap) }, Cmd.batch [(subCmd |> Cmd.map cmdMap);reloadCmd]

    match msg with
    | AuthMsg m ->
        match m, state.Page with
        | LoginMsg msg, Auth(Login state) -> Auth.Login.State.update msg state |> map (Login >> Auth) (LoginMsg >> Msg.AuthMsg) 
        | RegistrationMsg msg, Auth(Registration state) -> Auth.Registration.State.update msg state |> map (Registration >> Auth) (RegistrationMsg >> Msg.AuthMsg)
        | AccountActivationMsg msg, Auth(AccountActivation state) -> Auth.AccountActivation.State.update msg state |> map (AccountActivation >> Auth) (AccountActivationMsg >> Msg.AuthMsg)
        | ForgottenPasswordMsg msg, Auth(ForgottenPassword state) -> Auth.ForgottenPassword.State.update msg state |> map (ForgottenPassword >> Auth) (ForgottenPasswordMsg >> Msg.AuthMsg)
        | ResetPasswordMsg msg, Auth(ResetPassword state) -> Auth.ResetPassword.State.update msg state |> map (ResetPassword >> Auth) (ResetPasswordMsg >> Msg.AuthMsg)
        | _ -> state, Cmd.none
    | AdminMsg m ->
        match m, state.Page with
        | UsersMsg msg, Admin(Users state) -> Admin.Users.State.update msg state |> map (Users >> Admin) (UsersMsg >> Msg.AdminMsg)
        | LessonsMsg msg, Admin(Lessons state) -> Admin.Lessons.State.update msg state |> map (Lessons >> Admin) (LessonsMsg >> Msg.AdminMsg)
        | _ -> state, Cmd.none
    | CalendarMsg msg ->
        match state.Page with
        | Calendar state -> Calendar.State.update msg state |> mapWithReloadUser Calendar Msg.CalendarMsg
        | _ -> state, Cmd.none
    | MyLessonsMsg msg ->
        match state.Page with
        | MyLessons state -> MyLessons.State.update msg state |> map MyLessons MyLessonsMsg
        | _ -> state, Cmd.none
    | ReloadUser -> state, (TokenStorage.tryGetToken() |> Option.defaultValue "" |> Cmd.ofAsyncResult authAPI.GetUserByToken UserByTokenLoaded)
    | UserByTokenLoaded res ->
        match res with
        | Ok user -> { state with LoggedUser = Some user }, Cmd.none
        | Error _ -> state, LoggedOut |> Cmd.ofMsg
    | RefreshToken t -> state, (t |> Cmd.ofAsyncResult authAPI.RefreshToken TokenRefreshed)
    | TokenRefreshed res ->
        match res with
        | Ok t ->
            t |> TokenStorage.setToken
            state, Cmd.none
        | Error _ -> state, LoggedOut |> Cmd.ofMsg
    | LoggedOut ->
        TokenStorage.removeToken()
        { state with LoggedUser = None}, Navigation.newUrl Router.Routes.login
    | ToggleTermsView ->
        { state with TermsDisplayed = not state.TermsDisplayed }, Cmd.none

let subscribe (_:State) =
    let sub dispatch = 
        let timer = (TimeSpan.FromMinutes 1.).TotalMilliseconds |> int
        
        let handler _ =
            match TokenStorage.tryGetToken() with
            | Some t -> dispatch (RefreshToken t) |> ignore
            | None -> Cmd.none |> ignore
        Fable.Import.Browser.window.setInterval(handler, timer) |> ignore
    Cmd.ofSub sub