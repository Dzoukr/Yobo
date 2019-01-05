module Yobo.API.CompositionRoot.Communication

let private toAsync f = async { return f }

module Auth =
    open Yobo.API.Auth.Functions
    open Yobo.Libraries.Security 
    open Yobo.API.CompositionRoot
    open Yobo.Shared.Auth
    
    let api : Yobo.Shared.Auth.Communication.API = {
        GetToken = getToken Services.Users.authenticator.Login (Services.Users.authorizator.CreateToken >> fun x -> x.AccessToken) >> toAsync
        RefreshToken = refreshToken Services.Users.authorizator.ValidateToken (Services.Users.authorizator.CreateToken >> fun x -> x.AccessToken) >> toAsync
        GetUserByToken = getUser Services.Users.authorizator.ValidateToken >> toAsync
        ResendActivation = resendActivation Services.CommandHandler.handle >> toAsync
        Register = register Services.CommandHandler.handle Password.createHash >> toAsync
        ActivateAccount = activateAccount Services.CommandHandler.handle Services.Users.authenticator.GetByActivationKey >> toAsync
    }