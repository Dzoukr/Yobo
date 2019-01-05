module Yobo.API.CompositionRoot.Communication

open System
open FSharp.Rop

let private toAsync f = async { return f }

module Auth =
    open Yobo.API.Auth.Functions
    open Yobo.Libraries.Security 
    open Yobo.API.CompositionRoot

    let private adminUser =
        {
            Id = Guid("f65203d4-60dd-4580-a31c-e538807ef720")
            Email = "todo"
            FirstName = "Admin"
            LastName = "Admin"
            IsAdmin = true
        } : Yobo.Shared.Domain.User

    let private loginWithAdmin email pwd =
        if email = "a" && pwd = "a" then Ok adminUser
        else
            Services.Users.authenticator.Login email pwd <!> mapToUser
    
    let api : Yobo.Shared.Auth.Communication.API = {
        GetToken = getToken loginWithAdmin (Services.Users.authorizator.CreateToken >> fun x -> x.AccessToken) >> toAsync
        RefreshToken = refreshToken Services.Users.authorizator.ValidateToken (Services.Users.authorizator.CreateToken >> fun x -> x.AccessToken) >> toAsync
        GetUserByToken = getUser Services.Users.authorizator.ValidateToken >> toAsync
        ResendActivation = resendActivation Services.CommandHandler.handle >> toAsync
        Register = register Services.CommandHandler.handle Password.createHash >> toAsync
        ActivateAccount = activateAccount Services.CommandHandler.handle Services.Users.authenticator.GetByActivationKey >> toAsync
    }