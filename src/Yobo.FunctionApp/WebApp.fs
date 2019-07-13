module Yobo.FunctionApp.WebApp

open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Yobo.FunctionApp.Configuration

let frontend wwwRootPath =
    let wwwRootPath = if isNull wwwRootPath then "" else wwwRootPath
    Giraffe.ResponseWriters.htmlFile <| System.IO.Path.Combine(wwwRootPath, "index.html")

let auth (userSvc:Services.UsersServices) (cmdHandler:Pipeline.CommandHandler) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Auth.Communication.routeBuilder
    |> Remoting.fromValue (Communication.Auth.api userSvc cmdHandler)
    |> Remoting.buildHttpHandler

let admin (svc:Services.ApplicationServices) (cmdHandler:Pipeline.CommandHandler) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Admin.Communication.routeBuilder
    |> Remoting.fromValue (Communication.Admin.api svc cmdHandler)
    |> Remoting.buildHttpHandler

let calendar (svc:Services.ApplicationServices) (cmdHandler:Pipeline.CommandHandler) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Calendar.Communication.routeBuilder
    |> Remoting.fromValue (Communication.Calendar.api svc cmdHandler)
    |> Remoting.buildHttpHandler
    
let webApp (cfg :ApplicationConfiguration): HttpHandler =
    let services = cfg |> Services.createServices 
    let commandHandler = services |> Pipeline.getCommandHandler
    // register event handler
    services.EventStore.EventAppended.Add <| Pipeline.getEventHandler cfg services

    choose [ 
        auth services.Users commandHandler
        admin services commandHandler
        calendar services commandHandler
        frontend cfg.Server.WwwRootPath 
    ] 