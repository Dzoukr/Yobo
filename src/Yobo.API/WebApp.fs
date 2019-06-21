module Yobo.API.WebApp

open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Yobo.API.Configuration

let frontend wwwRootPath =
    let wwwRootPath = if isNull wwwRootPath then "" else wwwRootPath
    Giraffe.ResponseWriters.htmlFile <| System.IO.Path.Combine(wwwRootPath, "index.html")

let users : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Auth.Communication.routeBuilder
    |> Remoting.fromValue Yobo.API.CompositionRoot.Communication.Auth.api
    |> Remoting.buildHttpHandler

let admin : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Admin.Communication.routeBuilder
    |> Remoting.fromValue Yobo.API.CompositionRoot.Communication.Admin.api
    |> Remoting.buildHttpHandler

let calendar : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Calendar.Communication.routeBuilder
    |> Remoting.fromValue Yobo.API.CompositionRoot.Communication.Calendar.api
    |> Remoting.buildHttpHandler
    
let webApp (cfg :ApplicationConfiguration): HttpHandler =
    let services = cfg |> Services.createServices 
    let commandHandler = services |> Pipeline.getCommandHandler
    // register event handler
    services.EventStore.EventAppended.Add <| Pipeline.getEventHandler cfg services
    
    choose [ users; admin; calendar; frontend cfg.Server.WwwRootPath ] 