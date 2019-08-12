module Yobo.FunctionApp.Communication

open System
open FSharp.Rop
open Yobo.Shared.Communication
open Yobo.Shared.Domain

let private toAsync f = async { return f }

let private getUserById (userSvc:Yobo.FunctionApp.Services.AuthServices) i =
    if i = userSvc.AdminUser.Id then (Ok userSvc.AdminUser)
    else (userSvc.ReadQueries.GetById i |> Result.ofOption (ServerError.AuthError(Yobo.Shared.Auth.InvalidOrExpiredToken)))

module Security =
    open Yobo.Shared.Auth
    open Yobo.FunctionApp
    
    let onlyForLogged (userSvc:Services.AuthServices) (sp:SecuredParam<_>) =
        match sp.Token |> userSvc.Authorizator.ValidateToken with
        | Some claims ->
            claims |> Yobo.FunctionApp.Auth.Functions.claimsToUser (getUserById userSvc) <!> fun user -> (user, sp.Param)
        | None -> AuthError.InvalidOrExpiredToken |> ServerError.AuthError |> Error

    let onlyForAdmin userSvc (sp:SecuredParam<_>) =
        sp |> (onlyForLogged userSvc)
        >>= (fun (u,p) ->
            match u with
            | { IsAdmin = true } -> (u, p) |> Ok
            | _ -> AuthError.InvalidOrExpiredToken |> ServerError.AuthError |> Error
        )

module Auth =
    open Yobo.FunctionApp.Auth.Functions
    open Yobo.Libraries.Security 
    open Yobo.FunctionApp
    open Yobo.Core.Auth
    
    let api dbCtx emailSettings (svc:Services.ApplicationServices) : Yobo.Shared.Auth.Communication.API = 

        let withHandlers cmdHandler =
            let innerHandle proj cmd =
                cmd
                |> cmdHandler proj
                |> Result.mapError ServerError.DomainError
                <!> (fun evns ->
                    evns |> List.map (fun e -> DbEventHandler.handle e dbCtx) |> ignore
                    dbCtx.SubmitUpdates()

                    evns
                    |> List.choose (EmailEventHandler.handle svc.Auth.ReadQueries.GetById emailSettings)
                    |> Result.traverse svc.Emails.Send
                    |> ignore
                )
            innerHandle

        let loginWithAdmin (userSvc:Services.AuthServices) email pwd =
            if email = userSvc.AdminUser.Email && pwd = userSvc.AdminUserPassword then Ok userSvc.AdminUser
            else
                userSvc.Authenticator.Login email pwd
        
        {
            GetToken = getToken (loginWithAdmin svc.Auth) (svc.Auth.Authorizator.CreateToken >> fun x -> x.AccessToken) >> Result.mapError AuthError >> toAsync
            RefreshToken = refreshToken svc.Auth.Authorizator.ValidateToken (svc.Auth.Authorizator.CreateToken >> fun x -> x.AccessToken) >> toAsync
            GetUserByToken = getUser (getUserById svc.Auth) svc.Auth.Authorizator.ValidateToken >> toAsync
            ResendActivation = resendActivation (Projections.DbProjections.getById dbCtx) (withHandlers CommandHandler.regenerateActivationKey) >> toAsync
            Register = register (Projections.DbProjections.getAll dbCtx) (withHandlers CommandHandler.register) Password.createHash >> toAsync
            ActivateAccount = activateAccount (Projections.DbProjections.getByActivationKey dbCtx) (withHandlers CommandHandler.activate) >> toAsync
            InitiatePasswordReset = initiatePasswordReset (Projections.DbProjections.getByEmail dbCtx) (withHandlers CommandHandler.initiatePasswordReset)  >> toAsync
            ResetPassword = resetPassword (Projections.DbProjections.getByPasswordResetKey dbCtx) (withHandlers CommandHandler.resetPassword) Password.createHash >> toAsync
        }

module Admin =
    open Yobo.FunctionApp
    open Yobo.FunctionApp.Admin.Functions
    open Yobo.Core.Lessons

    let api dbCtx (svc:Services.ApplicationServices) : Yobo.Shared.Admin.Communication.API = 

        let withHandlers cmdHandler =
            let innerHandle proj cmd =
                cmd
                |> cmdHandler proj
                |> Result.mapError ServerError.DomainError
                <!> (fun evns ->
                    evns |> List.map (fun e -> DbEventHandler.handle e dbCtx) |> ignore
                    dbCtx.SubmitUpdates()
                )
            innerHandle

        {
            GetAllUsers = fun x -> x |> (Security.onlyForAdmin svc.Auth) <!> snd <!> svc.Auth.ReadQueries.GetAll |> toAsync
            AddCredits = fun x -> x |> (Security.onlyForAdmin svc.Auth) <!> snd >>= addCredits (Projections.DbProjections.getUserById dbCtx) (withHandlers CommandHandler.addCredits) |> toAsync
            GetLessonsForDateRange = fun x -> x |> (Security.onlyForAdmin svc.Auth) <!> snd <!> svc.Lessons.ReadQueries.GetAllForDateRange |> toAsync
            GetWorkshopsForDateRange = fun x -> x |> (Security.onlyForAdmin svc.Auth) <!> snd <!> svc.Workshops.ReadQueries.GetAllForDateRange |> toAsync
            AddLessons = fun x -> x |> (Security.onlyForAdmin svc.Auth) <!> snd >>= addLessons (Projections.DbProjections.getAllLessons dbCtx) (withHandlers CommandHandler.createLesson) |> toAsync
            AddWorkshops = fun x -> x |> (Security.onlyForAdmin svc.Auth) <!> snd >>= addWorkshops (Projections.DbProjections.getAllWorkshops dbCtx) (withHandlers CommandHandler.createWorkshop) |> toAsync
            CancelLesson = fun x -> x |> (Security.onlyForAdmin svc.Auth) <!> snd >>= cancelLesson (Projections.DbProjections.getLessonById dbCtx) (withHandlers CommandHandler.cancelLesson) |> toAsync
            DeleteWorkshop = fun x -> x |> (Security.onlyForAdmin svc.Auth) <!> snd >>= deleteWorkshop (Projections.DbProjections.getWorkshopById dbCtx) (withHandlers CommandHandler.deleteWorkshop) |> toAsync
        }

module Calendar =
    open Yobo.FunctionApp
    open Yobo.Shared.Calendar.Domain
    open Yobo.FunctionApp.Calendar.Functions
    open Yobo.Core.Lessons

    let private limit (st,en) =
        let lower = Yobo.Shared.DateRange.getDateRangeForWeekOffset maxLowerOffset |> fst
        let st = if st < lower then lower else st
        st,en

    let api dbCtx (svc:Services.ApplicationServices) : Yobo.Shared.Calendar.Communication.API =

        let withHandlers cmdHandler =
            let innerHandle proj cmd =
                cmd
                |> cmdHandler proj
                |> Result.mapError ServerError.DomainError
                <!> (fun evns ->
                    evns |> List.map (fun e -> DbEventHandler.handle e dbCtx) |> ignore
                    dbCtx.SubmitUpdates()
                )
            innerHandle

        let withUser fn (u,cmd) = (u,cmd) |> fn

        {
            GetWorkshopsForDateRange = fun x -> x |> (Security.onlyForLogged svc.Auth) <!> (snd >> limit) <!> svc.Workshops.ReadQueries.GetAllForDateRange |> toAsync
            GetLessonsForDateRange =
                (fun x ->
                    x |> (Security.onlyForLogged svc.Auth)
                    <!> (fun (u,p) ->
                        p 
                        |> limit
                        |> svc.Lessons.ReadQueries.GetAllForDateRange |> List.map (Lesson.FromAdminLesson u.Id)
                    ) 
                    |> toAsync
                )
            CancelReservation = fun x -> x |> (Security.onlyForLogged svc.Auth) >>= cancelReservation (Projections.DbProjections.getLessonById dbCtx) (withHandlers CommandHandler.cancelReservation) |> toAsync 
            AddReservation = fun x -> x |> (Security.onlyForLogged svc.Auth) >>= addReservation (Projections.DbProjections.getLessonById dbCtx) (Projections.DbProjections.getUserById dbCtx) (withHandlers CommandHandler.addReservation)  |> toAsync
        }

module MyLessons =
    let api dbCtx (svc:Services.ApplicationServices) : Yobo.Shared.MyLessons.Communication.API = 
        {
            GetMyLessons = 
                (fun x -> 
                    x |> (Security.onlyForLogged svc.Auth)
                    <!> (fun (u,_) -> 
                        Yobo.Core.Lessons.ReadQueries.MyLessons.getMyLessons dbCtx u.Id
                    ) 
                    |> toAsync
                ) 
        }