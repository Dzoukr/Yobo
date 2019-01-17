module Yobo.Client.Router

open System
open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation
open Fable.Import
open Fable.Helpers
open Fable.Core.JsInterop

module Routes =
    let defaultPage = "/users"
    let login = "/login"
    let registration = "/registration"
    let users = "/users"
    let lessons = "/lessons"

let goToUrl (e: React.MouseEvent) =
    e.preventDefault()
    let href = !!e.currentTarget?href
    Navigation.newUrl href |> List.map (fun f -> f ignore) |> ignore