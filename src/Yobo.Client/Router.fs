module Yobo.Client.Router

open System
open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation
open Fable.Import
open Fable.Helpers
open Fable.Core.JsInterop

type AuthPage =
    | Login
    | Logout
    | Registration
    | ForgottenPassword
    | AccountActivation of Guid
    with
        member x.ToPath() = 
            match x with
            | Login -> "/login"
            | Logout -> "/logout"
            | Registration -> "/registration"
            | ForgottenPassword -> "/forgottenPassword"
            | AccountActivation id -> sprintf "/accountActivation/%A" id

type AdminPage =
    | Users
    | Lessons
    with
        member x.ToPath() = 
            match x with
            | Users -> "/users"
            | Lessons -> "/lessons"

type Page =
    | Auth of AuthPage
    | Admin of AdminPage
     with
        member x.ToPath() =
            match x with
            | Auth p -> p.ToPath()
            | Admin p -> p.ToPath()
        static member DefaultPage = Users |> Admin

let pageParser: Parser<Page -> Page, Page> =
    oneOf [
        map (Auth(Login)) (s "login")
        map (Auth(Logout)) (s "logout")
        map (Auth(Registration)) (s "registration")
        map (Auth(ForgottenPassword)) (s "forgottenPassword")
        map ((fun (x:string) -> Guid(x)) >> AccountActivation >> Auth) (s "accountActivation" </> str)

        map (Admin(Users)) (s "users")
        map (Admin(Lessons)) (s "lessons")
    ]

let modifyUrl (route:Page) = route.ToPath() |> Navigation.modifyUrl
let newUrl (route:Page) = route.ToPath() |> Navigation.newUrl

let goToUrl (e: React.MouseEvent) =
    e.preventDefault()
    let href = !!e.currentTarget?href
    Navigation.newUrl href |> List.map (fun f -> f ignore) |> ignore