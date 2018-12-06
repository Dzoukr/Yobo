module Yobo.API.Routes

open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe

let frontend wwwRootPath =
    let wwwRootPath = if isNull wwwRootPath then "" else wwwRootPath
    Giraffe.ResponseWriters.htmlFile <| System.IO.Path.Combine(wwwRootPath, "index.html")

let registration : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Registration.Communication.routeBuilder
    |> Remoting.fromValue Yobo.API.CompositionRoot.HttpHandlers.Registration.api
    |> Remoting.buildHttpHandler

let webApp wwwRootPath : HttpHandler = choose [ registration; frontend wwwRootPath ]