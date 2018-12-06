module Yobo.API.CompositionRoot.HttpHandlers

open System
open Giraffe
open Yobo.Shared.Communication
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2

let private toAsync f = async { return f }

module Registration =
    open Yobo.API.Registration.HttpHandlers
    open Yobo.Shared.Registration.Domain
    open Yobo.Libraries.Security
    open Yobo.API.CompositionRoot

    let api : Yobo.Shared.Registration.Communication.API = {
        Register = register Services.CommandHandler.handle Password.createHash >> toAsync
        ActivateAccount = activateAccount Services.CommandHandler.handle Services.Users.queries.GetByActivationKey >> toAsync
    }

module Login =
    open Yobo.API.Login.HttpHandlers
    open Yobo.Shared.Login.Domain
    open Yobo.API.CompositionRoot
    open Yobo.Libraries.Security 
    
    //let login : HttpHandler = safeBindJson<Login> (login (Services.Users.authenticator.Login Password.verifyPassword) >> toHandler)
