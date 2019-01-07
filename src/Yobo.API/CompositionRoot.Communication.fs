module Yobo.API.CompositionRoot.Communication

open System
open FSharp.Rop

let private toAsync f = async { return f }

module Auth =
    open Yobo.API.Auth.Functions
    open Yobo.Libraries.Security 
    open Yobo.API.CompositionRoot
    open Yobo.API
    open Yobo.Shared.Communication
    open Yobo.Shared.Auth
    open Yobo.Shared.Auth.Domain

    let private adminUser =
        {
            Id = Guid("f65203d4-60dd-4580-a31c-e538807ef720")
            Email = Configuration.Admin.email
            FirstName = "Admin"
            LastName = "Admin"
            IsAdmin = true
        } : LoggedUser

    let private loginWithAdmin email pwd =
        if email = Configuration.Admin.email && pwd = Configuration.Admin.password then Ok adminUser
        else
            Services.Users.authenticator.Login email pwd <!> mapToLoggedUser

    let onlyForAdmin (sp:SecuredParam<_>) =
        match sp.Token |> Services.Users.authorizator.ValidateToken with
        | Some claims ->
            match claims |> claimsToUser with
            | { IsAdmin = true } -> Ok sp.Param
            | _ -> AuthError.InvalidOrExpiredToken |> ServerError.AuthError |> Error
        | None -> AuthError.InvalidOrExpiredToken |> ServerError.AuthError |> Error

    let api : Yobo.Shared.Auth.Communication.API = {
        GetToken = getToken loginWithAdmin (Services.Users.authorizator.CreateToken >> fun x -> x.AccessToken) >> toAsync
        RefreshToken = refreshToken Services.Users.authorizator.ValidateToken (Services.Users.authorizator.CreateToken >> fun x -> x.AccessToken) >> toAsync
        GetUserByToken = getUser Services.Users.authorizator.ValidateToken >> toAsync
        ResendActivation = resendActivation Services.CommandHandler.handle >> toAsync
        Register = register Services.CommandHandler.handle Password.createHash >> toAsync
        ActivateAccount = activateAccount Services.CommandHandler.handle Services.Users.authenticator.GetByActivationKey >> toAsync
    }

module Admin =
    open Yobo.API.Admin.Functions
    open Yobo.API.CompositionRoot
    open Yobo.API
    
    let api : Yobo.Shared.Admin.Communication.API = {
        GetAllUsers = fun x -> x |> Auth.onlyForAdmin >>= Services.Users.queries.GetAll <!> List.map mapToUser |> toAsync
    }