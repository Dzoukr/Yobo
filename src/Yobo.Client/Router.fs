module Yobo.Client.Router

open System
open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation
open Fable.Import
open Fable.Helpers
open Fable.Core.JsInterop

type Page =
    | Login
    | Register
    | ForgottenPassword
    | ActivateAccount of Guid
    with
        member x.ToPath() = 
            match x with
            | Login -> "/login"
            | Register -> "/register"
            | ForgottenPassword -> "/forgottenPassword"
            | ActivateAccount id -> sprintf "/activateAccount/%A" id

let pageParser: Parser<Page -> Page, Page> =
    oneOf [
        map Login (s "login")
        map Register (s "register")
        map ForgottenPassword (s "forgottenPassword")
        map ((fun (x:string) -> Guid(x)) >> ActivateAccount) (s "activateAccount" </> str)
    ]

let modifyUrl (route:Page) = route.ToPath() |> Navigation.modifyUrl
let modifyLocation (route:Page) = Browser.window.location.href <- route.ToPath()

let goToUrl (e: React.MouseEvent) =
    e.preventDefault()
    let href = !!e.target?href
    Navigation.newUrl href |> List.map (fun f -> f ignore) |> ignore