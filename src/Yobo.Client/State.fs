module Yobo.Client.State

open Elmish
open Fable.Import
open Yobo.Client.Domain
open Http
open System
open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation
open Router

let private removeSlash (s:string) = s.TrimStart([|'/'|])
let inline private s p = p |> removeSlash |> s

let pageParser: Parser<Page -> Page, Page> =
    oneOf [
        map (AuthPage(ForgottenPassword)) (s ForgottenPassword.Path)
        map (AuthPage(Login)) (s Login.Path)
        map (AuthPage(Registration)) (s Registration.Path)
        map ((fun (x:string) -> Guid(x)) >> AccountActivation >> AuthPage) (s "/accountActivation" </> str)
        map ((fun (x:string) -> Guid(x)) >> ResetPassword >> AuthPage) (s "/resetPassword" </> str)
        map (AdminPage(Users)) (s Users.Path)
        map (AdminPage(Lessons)) (s Lessons.Path)
        map Calendar (s Page.Calendar.Path)
        map MyLessons (s MyLessons.Path)
    ]

let private withCheckingLogin (state:State) cmd =
    let requiresLogin =
        match state.Page with
        | AuthPage _ -> false
        | _ -> true

    if requiresLogin then
        match state.LoggedUser, TokenStorage.tryGetToken() with
        | None, Some _ -> [ Cmd.ofMsg Msg.ReloadUser; cmd ] |> Cmd.batch
        | Some _, Some _ -> cmd
        | _ -> LoggedOut |> Cmd.ofMsg
    else cmd

let urlUpdate (result: Option<Page>) state =
    match result with
    | None -> state, Navigation.newUrl Router.Page.Default.Path
    | Some page ->
        let state = { state with Page = page }
        let cmd =
            match page with
            | AuthPage pg ->
                match pg with
                | AccountActivation id ->
                    id
                    |> Auth.AccountActivation.Domain.Msg.Activate
                    |> AuthMsg.AccountActivationMsg
                    |> AuthMsg
                    |> Cmd.ofMsg
                | ResetPassword id ->
                    id
                    |> Auth.ResetPassword.Domain.Msg.Init
                    |> AuthMsg.ResetPasswordMsg
                    |> AuthMsg
                    |> Cmd.ofMsg
                | _ -> Cmd.none
            | AdminPage pg ->
                match pg with
                | Users _ -> Admin.Users.Domain.Msg.Init |> UsersMsg |> AdminMsg |> Cmd.ofMsg
                | Lessons _ -> Admin.Lessons.Domain.Msg.Init |> LessonsMsg |> AdminMsg |> Cmd.ofMsg
            | Calendar _ -> Calendar.Domain.Msg.Init |> CalendarMsg |> Cmd.ofMsg
            | MyLessons _ -> MyLessons.Domain.Msg.Init |> MyLessonsMsg |> Cmd.ofMsg
        state, (withCheckingLogin state cmd)

let init result =
    urlUpdate result State.Init
 
let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    let map stateMap cmdMap (subState,subCmd) = { state with States = (stateMap state.States subState) }, (subCmd |> Cmd.map cmdMap)

    let mapWithReloadUser stateMap cmdMap (subState,subCmd,reloadUser) =
        let reloadCmd = if reloadUser then ReloadUser |> Cmd.ofMsg else Cmd.none
        { state with States = (stateMap state.States subState) }, Cmd.batch [(subCmd |> Cmd.map cmdMap); reloadCmd]

    match msg with
    | AuthMsg (LoginMsg msg) ->
        Auth.Login.State.update msg state.States.Login |> map (fun s ns -> { s with Login = ns }) (LoginMsg >> Msg.AuthMsg) 
    | AuthMsg (RegistrationMsg msg) ->
        Auth.Registration.State.update msg state.States.Registration |> map (fun s ns -> { s with Registration = ns }) (RegistrationMsg >> Msg.AuthMsg) 
    | AuthMsg (AccountActivationMsg msg) ->
        Auth.AccountActivation.State.update msg state.States.AccountActivation |> map (fun s ns -> { s with AccountActivation = ns }) (AccountActivationMsg >> Msg.AuthMsg) 
    | AuthMsg (ForgottenPasswordMsg msg) ->
        Auth.ForgottenPassword.State.update msg state.States.ForgottenPassword |> map (fun s ns -> { s with ForgottenPassword = ns }) (ForgottenPasswordMsg >> Msg.AuthMsg)
    | AuthMsg (ResetPasswordMsg msg) ->
        Auth.ResetPassword.State.update msg state.States.ResetPassword |> map (fun s ns -> { s with ResetPassword = ns }) (ResetPasswordMsg >> Msg.AuthMsg)
    | AdminMsg (UsersMsg msg) -> Admin.Users.State.update msg state.States.Users |> map (fun s ns -> { s with Users = ns }) (UsersMsg >> Msg.AdminMsg)
    | AdminMsg (LessonsMsg msg) -> Admin.Lessons.State.update msg state.States.Lessons |> map (fun s ns -> { s with Lessons = ns }) (LessonsMsg >> Msg.AdminMsg)
    | CalendarMsg msg -> Calendar.State.update msg state.States.Calendar |> mapWithReloadUser (fun s ns -> { s with Calendar = ns }) Msg.CalendarMsg
    | MyLessonsMsg msg -> MyLessons.State.update msg state.States.MyLessons |> map (fun s ns -> { s with MyLessons = ns }) Msg.MyLessonsMsg
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
        { state with LoggedUser = None}, Navigation.newUrl AuthPage.Login.Path
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