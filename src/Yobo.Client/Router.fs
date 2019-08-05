module Yobo.Client.Router

open System
open Elmish.Navigation
open Fable.Import
open Fable.Core.JsInterop
open Browser.Types
open Elmish.UrlParser

type AuthPage =
    | Login
    | Registration
    | ForgottenPassword
    | ResetPassword of Guid
    | AccountActivation of Guid
    with
        member x.Path = 
            match x with
            | Login -> "/login"
            | Registration -> "/registration"
            | ForgottenPassword -> "/forgottenPassword"
            | ResetPassword id -> sprintf "/resetPassword/%A" id
            | AccountActivation id -> sprintf "/accountActivation/%A" id

type AdminPage =
    | Users
    | Lessons
    with
        member x.Path = 
            match x with
            | Users -> "/users"
            | Lessons -> "/lessons"

type Page =
    | Calendar
    | MyLessons
    | AuthPage of AuthPage
    | AdminPage of AdminPage
    with
        member x.Path = 
            match x with
            | Calendar -> "/calendar"
            | MyLessons -> "/mylessons"
            | AuthPage p -> p.Path
            | AdminPage p -> p.Path
        static member Default = Calendar


let goToUrl (e: MouseEvent) =
    e.preventDefault()
    let href = !!e.currentTarget?href
    Navigation.newUrl href |> List.map (fun f -> f ignore) |> ignore