module Yobo.API.CompositionRoot.Communication

let private toAsync f = async { return f }

module Registration =
    open Yobo.API.Registration.Functions
    open Yobo.Libraries.Security
    open Yobo.API.CompositionRoot

    let api : Yobo.Shared.Registration.Communication.API = {
        Register = register Services.CommandHandler.handle Password.createHash >> toAsync
        ActivateAccount = activateAccount Services.CommandHandler.handle Services.Users.queries.GetByActivationKey >> toAsync
    }

module Login =
    open Yobo.API.Login.Functions
    open Yobo.Libraries.Security 
    open Yobo.API.CompositionRoot

    let api : Yobo.Shared.Login.Communication.API = {
        Login = login (Services.Users.authenticator.Login Password.verifyPassword) >> toAsync
        ResendActivation = resendActivation Services.CommandHandler.handle >> toAsync
    }