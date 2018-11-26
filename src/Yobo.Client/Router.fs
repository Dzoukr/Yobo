module Yobo.Client.Router

open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation
open Fable.Import

type Page =
    | Login
    | Register
    | ForgottenPassword
    with
        member x.ToPath() = 
            match x with
            | Login -> "/login"
            | Register -> "/register"
            | ForgottenPassword -> "/forgottenPassword"

let pageParser: Parser<Page -> Page, Page> =
    oneOf [
        map Login (s "login")
        map Register (s "register")
        map ForgottenPassword (s "forgottenPassword")
    ]

let modifyUrl (route:Page) = route.ToPath() |> Navigation.modifyUrl
let modifyLocation (route:Page) = Browser.window.location.href <- route.ToPath()