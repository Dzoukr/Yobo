module Yobo.FunctionApp.Services
open Yobo.Libraries.Security
open Yobo.Core.Auth
open Yobo.Libraries.Authorization

type AuthServices = {
    ReadQueries : ReadQueries.UserQueries
    Authenticator : Authenticator.Authenticator
    Authorizator : Authorizator
    AdminUser : Yobo.Shared.Domain.User
    AdminUserPassword : string
}

let createAuthService (adminConf:Configuration.AdminConfiguration) (conf:Configuration) (connString:string) = {
    ReadQueries = connString |> ReadQueries.createDefault
    Authenticator = Password.verifyPassword |> Authenticator.createDefault connString
    Authorizator = conf |> Jwt.createAuthorizator
    AdminUser = {
        Id = System.Guid("f65203d4-60dd-4580-a31c-e538807ef720")
        Email = adminConf.Email
        FirstName = "Admin"
        LastName = "Admin"
        IsAdmin = true
        Activated = Some System.DateTimeOffset.MinValue
        Credits = 0
        CreditsExpiration = None
        CashReservationBlockedUntil = None
    }
    AdminUserPassword = adminConf.Password
}

open Yobo.Core.Lessons

type LessonsServices = {
    ReadQueries : ReadQueries.Lessons.LessonsQueries
}

let createLessonsService connString = {
    ReadQueries = connString |> ReadQueries.Lessons.createDefault
}

open Yobo.Libraries.Emails

type WorkshopsServices = {
    ReadQueries : ReadQueries.Workshops.WorkshopsQueries
}

let createWorkshopsService connString = {
    ReadQueries = connString |> ReadQueries.Workshops.createDefault
}

type ApplicationServices = {
    Auth : AuthServices
    Lessons : LessonsServices
    Workshops : WorkshopsServices
    Emails : Yobo.Libraries.Emails.EmailProvider
}

let createServices (conf:Configuration.ApplicationConfiguration) : ApplicationServices =
    {
        Auth = createAuthService conf.Admin conf.Authorization conf.ReadDbConnectionString
        Lessons = createLessonsService conf.ReadDbConnectionString
        Workshops = createWorkshopsService conf.ReadDbConnectionString
        Emails = conf.Emails.Mailjet |> MailjetProvider.create
    }