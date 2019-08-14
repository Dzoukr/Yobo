module Yobo.FunctionApp.WebApp

open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Yobo.FunctionApp.Configuration
open Yobo.Core
open Serilog

let auth dbCtx emailSettings (svc:Services.ApplicationServices) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Auth.Communication.routeBuilder
    |> Remoting.fromValue (Communication.Auth.api dbCtx emailSettings svc)
    |> Remoting.buildHttpHandler

let admin dbCtx (svc:Services.ApplicationServices) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Admin.Communication.routeBuilder
    |> Remoting.fromValue (Communication.Admin.api dbCtx svc)
    |> Remoting.buildHttpHandler

let calendar dbCtx (svc:Services.ApplicationServices) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Calendar.Communication.routeBuilder
    |> Remoting.fromValue (Communication.Calendar.api dbCtx svc)
    |> Remoting.buildHttpHandler

let myLessons dbCtx (svc:Services.ApplicationServices) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.MyLessons.Communication.routeBuilder
    |> Remoting.fromValue (Communication.MyLessons.api dbCtx svc)
    |> Remoting.buildHttpHandler
    
let webApp (cfg :ApplicationConfiguration): HttpHandler =
    let services = cfg |> Services.createServices 
    let dbCtx = cfg.ReadDbConnectionString |> Yobo.Core.ReadDb.Db.GetDataContext
    let emailSettings : EmailSettings.Settings = { From = cfg.Emails.From; BaseUrl = cfg.Server.BaseUrl }

    choose [ 
        auth dbCtx emailSettings services
        admin dbCtx services
        calendar dbCtx services
        myLessons dbCtx services
    ] 