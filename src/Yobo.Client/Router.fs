module Yobo.Client.Router

open System
open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation
open Fable.Import
open Fable.Helpers
open Fable.Core.JsInterop

type PublicPage =
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

type Page =
    | Public of PublicPage
     with
        member x.ToPath() =
            match x with
            | Public p -> p.ToPath()

let pageParser: Parser<Page -> Page, Page> =
    oneOf [
        map (Public(Login)) (s "login")
        map (Public(Registration)) (s "registration")
        map (Public(ForgottenPassword)) (s "forgottenPassword")
        map ((fun (x:string) -> Guid(x)) >> AccountActivation >> Public) (s "accountActivation" </> str)
    ]

let modifyUrl (route:Page) = route.ToPath() |> Navigation.modifyUrl
let newUrl (route:Page) = route.ToPath() |> Navigation.newUrl
//let modifyLocation (route:Page) = Browser.window.location.href <- route.ToPath()

let goToUrl (e: React.MouseEvent) =
    e.preventDefault()
    let href = !!e.target?href
    Navigation.newUrl href |> List.map (fun f -> f ignore) |> ignore