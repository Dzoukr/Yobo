module Yobo.API.Routes

open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe

let frontend wwwRootPath =
    let wwwRootPath = if isNull wwwRootPath then "" else wwwRootPath
    Giraffe.ResponseWriters.htmlFile <| System.IO.Path.Combine(wwwRootPath, "index.html")

let users : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Auth.Communication.routeBuilder
    |> Remoting.fromValue Yobo.API.CompositionRoot.Communication.Auth.api
    |> Remoting.buildHttpHandler

let webApp wwwRootPath : HttpHandler =
    choose [ users; frontend wwwRootPath ] 