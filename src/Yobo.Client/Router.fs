module Yobo.Client.Router

open System
open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation
open Fable.Import
open Fable.Helpers
open Fable.Core.JsInterop

type Page =
    | Login
    | Registration
    | ForgottenPassword
    | AccountActivation of Guid
    with
        member x.ToPath() = 
            match x with
            | Login -> "/login"
            | Registration -> "/registration"
            | ForgottenPassword -> "/forgottenPassword"
            | AccountActivation id -> sprintf "/accountActivation/%A" id

let pageParser: Parser<Page -> Page, Page> =
    oneOf [
        map Login (s "login")
        map Registration (s "registration")
        map ForgottenPassword (s "forgottenPassword")
        map ((fun (x:string) -> Guid(x)) >> AccountActivation) (s "accountActivation" </> str)
    ]

let modifyUrl (route:Page) = route.ToPath() |> Navigation.modifyUrl
let modifyLocation (route:Page) = Browser.window.location.href <- route.ToPath()

let goToUrl (e: React.MouseEvent) =
    e.preventDefault()
    let href = !!e.target?href
    Navigation.newUrl href |> List.map (fun f -> f ignore) |> ignore