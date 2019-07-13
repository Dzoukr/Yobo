module Yobo.API.Communication

open System
open FSharp.Rop
open Yobo.Shared.Communication
open Yobo.Shared.Domain

let private toAsync f = async { return f }

let private getUserById (userSvc:Yobo.API.Services.UsersServices) i =
    if i = userSvc.AdminUser.Id then (Ok userSvc.AdminUser)
    else (userSvc.ReadQueries.GetById i |> Result.ofOption (ServerError.AuthError(Yobo.Shared.Auth.InvalidOrExpiredToken)))

module Security =
    open Yobo.Shared.Auth
    open Yobo.API

    let handleForUser (cmdHandler:Yobo.API.Pipeline.CommandHandler) f (user:User, param) =
        let handleFn = cmdHandler.HandleForUser user.Id
        f handleFn param

    let handleForUserWithId (cmdHandler:Yobo.API.Pipeline.CommandHandler) f (user:User, param) =
        let handleFn = cmdHandler.HandleForUser user.Id
        f user.Id handleFn param

    let onlyForLogged (userSvc:Services.UsersServices) (sp:SecuredParam<_>) =
        match sp.Token |> userSvc.Authorizator.ValidateToken with
        | Some claims ->
            claims |> Yobo.API.Auth.Functions.claimsToUser (getUserById userSvc) <!> fun user -> (user, sp.Param)
        | None -> AuthError.InvalidOrExpiredToken |> ServerError.AuthError |> Error

    let onlyForAdmin userSvc (sp:SecuredParam<_>) =
        sp |> (onlyForLogged userSvc)
        >>= (fun (u,p) ->
            match u with
            | { IsAdmin = true } -> (u, p) |> Ok
            | _ -> AuthError.InvalidOrExpiredToken |> ServerError.AuthError |> Error
        )

module Auth =
    open Yobo.API.Auth.Functions
    open Yobo.Libraries.Security 
    open Yobo.API

    
    let api (userSvc:Services.UsersServices) (cmdHandler:Pipeline.CommandHandler) : Yobo.Shared.Auth.Communication.API = 
        
        let loginWithAdmin (userSvc:Services.UsersServices) email pwd =
            if email = userSvc.AdminUser.Email && pwd = userSvc.AdminUserPassword then Ok userSvc.AdminUser
            else
                userSvc.Authenticator.Login email pwd

        {
            GetToken = getToken (loginWithAdmin userSvc) (userSvc.Authorizator.CreateToken >> fun x -> x.AccessToken) >> Result.mapError AuthError >> toAsync
            RefreshToken = refreshToken userSvc.Authorizator.ValidateToken (userSvc.Authorizator.CreateToken >> fun x -> x.AccessToken) >> toAsync
            GetUserByToken = getUser (getUserById userSvc) userSvc.Authorizator.ValidateToken >> toAsync
            ResendActivation = resendActivation cmdHandler.HandleAnonymous >> toAsync
            Register = register cmdHandler.HandleAnonymous Password.createHash >> toAsync
            ActivateAccount = activateAccount cmdHandler.HandleAnonymous (userSvc.Authenticator.GetByActivationKey >> Result.ofOption (AuthError(Yobo.Shared.Auth.ActivationKeyDoesNotMatch))) >> toAsync
            InitiatePasswordReset = initiatePasswordReset cmdHandler.HandleAnonymous (userSvc.Authenticator.GetByEmail >> Result.ofOption (AuthError(Yobo.Shared.Auth.InvalidLogin))) >> toAsync
            ResetPassword = resetPassword cmdHandler.HandleAnonymous Password.createHash (userSvc.Authenticator.GetByPasswordResetKey >> Result.ofOption (AuthError(Yobo.Shared.Auth.PasswordResetKeyDoesNotMatch))) >> toAsync
        }

module Admin =
    open Yobo.API
    open Yobo.API.Admin.Functions

    let api (svc:Services.ApplicationServices) (cmdHandler:Pipeline.CommandHandler) : Yobo.Shared.Admin.Communication.API = 
        {
            GetAllUsers = fun x -> x |> (Security.onlyForAdmin svc.Users) <!> snd <!> svc.Users.ReadQueries.GetAll |> toAsync
            AddCredits = fun x -> x |> (Security.onlyForAdmin svc.Users) >>= (Security.handleForUser cmdHandler) addCredits |> toAsync
            GetLessonsForDateRange = fun x -> x |> (Security.onlyForAdmin svc.Users) <!> snd <!> svc.Lessons.ReadQueries.GetAllForDateRange |> toAsync
            GetWorkshopsForDateRange = fun x -> x |> (Security.onlyForAdmin svc.Users) <!> snd <!> svc.Workshops.ReadQueries.GetAllForDateRange |> toAsync
            AddLessons = fun x -> x |> (Security.onlyForAdmin svc.Users) >>= (Security.handleForUser cmdHandler) addLessons |> toAsync
            AddWorkshops = fun x -> x |> (Security.onlyForAdmin svc.Users) >>= (Security.handleForUser cmdHandler) addWorkshops |> toAsync
            CancelLesson = fun x -> x |> (Security.onlyForAdmin svc.Users) >>= (Security.handleForUser cmdHandler) cancelLesson |> toAsync
            DeleteWorkshop = fun x -> x |> (Security.onlyForAdmin svc.Users) >>= (Security.handleForUser cmdHandler) deleteWorkshop |> toAsync
        }

module Calendar =
    open Yobo.API
    open Yobo.Shared.Calendar.Domain
    open Yobo.API.Calendar.Functions

    let api (svc:Services.ApplicationServices) (cmdHandler:Pipeline.CommandHandler) : Yobo.Shared.Calendar.Communication.API = {
        GetWorkshopsForDateRange = fun x -> x |> (Security.onlyForLogged svc.Users) <!> snd <!> svc.Workshops.ReadQueries.GetAllForDateRange |> toAsync
        GetLessonsForDateRange =
            (fun x ->
                x |> (Security.onlyForLogged svc.Users)
                <!> (fun (u,p) ->
                    p |> svc.Lessons.ReadQueries.GetAllForDateRange |> List.map (Lesson.FromAdminLesson u.Id)
                ) 
                |> toAsync
            )
        AddReservation = fun x -> x |> (Security.onlyForLogged svc.Users) >>= (Security.handleForUserWithId cmdHandler) addReservation |> toAsync
        CancelReservation = fun x -> x |> (Security.onlyForLogged svc.Users) >>= (Security.handleForUserWithId cmdHandler) cancelReservation |> toAsync 
    }