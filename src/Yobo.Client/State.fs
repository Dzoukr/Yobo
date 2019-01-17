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
        map (Auth(Login(Auth.Login.Domain.State.Init))) (s "login")
        map (Auth(Registration(Auth.Registration.Domain.State.Init))) (s "registration")
        map ((fun (x:string) -> Guid(x)) >> Auth.AccountActivation.Domain.State.Init >> AccountActivation >> Auth) (s "accountActivation" </> str)
        map (Admin(Users(Admin.Users.Domain.State.Init))) (s "users")
        map (Admin(Lessons(Admin.Lessons.Domain.State.Init))) (s "lessons") ]

let private withCheckingLogin (state:State) cmd =
    let requiresLogin =
        match state.Page with
        | Auth _ -> false
        | Admin _ -> true

    if requiresLogin then
        match state.LoggedUser, TokenStorage.tryGetToken() with
        | None, Some t -> [ Cmd.ofMsg (Msg.LoadUserByToken t); cmd ] |> Cmd.batch
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
        (state |> withRoute), (withCheckingLogin state cmd)

let init result =
    urlUpdate result State.Init
 
let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    let map stateMap cmdMap (subState,subCmd) =
        { state with Page = (subState |> stateMap) }, (subCmd |> Cmd.map cmdMap)

    match msg with
    | AuthMsg m ->
        match m, state.Page with
        | LoginMsg msg, Auth(Login state) -> Auth.Login.State.update msg state |> map (Login >> Auth) (LoginMsg >> Msg.AuthMsg) 
        | RegistrationMsg msg, Auth(Registration state) -> Auth.Registration.State.update msg state |> map (Registration >> Auth) (RegistrationMsg >> Msg.AuthMsg)
        | AccountActivationMsg msg, Auth(AccountActivation state) -> Auth.AccountActivation.State.update msg state |> map (AccountActivation >> Auth) (AccountActivationMsg >> Msg.AuthMsg)
        | _ -> state, Cmd.none
    | AdminMsg m ->
        match m, state.Page with
        | UsersMsg msg, Admin(Users state) -> Admin.Users.State.update msg state |> map (Users >> Admin) (UsersMsg >> Msg.AdminMsg)
        | LessonsMsg msg, Admin(Lessons state) -> Admin.Lessons.State.update msg state |> map (Lessons >> Admin) (LessonsMsg >> Msg.AdminMsg)
        | _ -> state, Cmd.none
    | LoadUserByToken t -> state, (t |> Cmd.ofAsyncResult authAPI.GetUserByToken UserByTokenLoaded)
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

let subscribe (_:State) =
    let sub dispatch = 
        let timer = (TimeSpan.FromMinutes 1.).TotalMilliseconds |> int
        
        let handler _ =
            match TokenStorage.tryGetToken() with
            | Some t -> dispatch (RefreshToken t) |> ignore
            | None -> Cmd.none |> ignore
        Fable.Import.Browser.window.setInterval(handler, timer) |> ignore
    Cmd.ofSub sub