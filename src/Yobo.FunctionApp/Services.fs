module Yobo.FunctionApp.Services
open Yobo.Libraries.Security
open Yobo.Core.Users
open Yobo.Libraries.Authorization

type UsersServices = {
    ReadQueries : ReadQueries.UserQueries
    Authenticator : Authenticator.Authenticator
    Authorizator : Authorizator
    AdminUser : Yobo.Shared.Domain.User
    AdminUserPassword : string
}

let createUsersService (adminConf:Configuration.AdminConfiguration) (conf:Configuration) (connString:string) = {
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
    ReadQueries : ReadQueries.LessonsQueries
}

let createLessonsService connString = {
    ReadQueries = connString |> ReadQueries.createDefault
}

open Yobo.Core.Workshops
open Yobo.Libraries.Emails

type WorkshopsServices = {
    ReadQueries : ReadQueries.WorkshopsQueries
}

let createWorkshopsService connString = {
    ReadQueries = connString |> ReadQueries.createDefault
}

type ApplicationServices = {
    Users : UsersServices
    Lessons : LessonsServices
    Workshops : WorkshopsServices
    Emails : Yobo.Libraries.Emails.EmailProvider
    EventStore : CosmoStore.EventStore
    SymetricCryptoProvider : SymetricCryptoProvider.SymetricCryptoProvider
}

let createServices (conf:Configuration.ApplicationConfiguration) : ApplicationServices =
    {
        Users = createUsersService conf.Admin conf.Authorization conf.ReadDbConnectionString
        Lessons = createLessonsService conf.ReadDbConnectionString
        Workshops = createWorkshopsService conf.ReadDbConnectionString
        Emails = conf.Emails.Mailjet |> MailjetProvider.create
        EventStore = conf.EventStore |> CosmoStore.TableStorage.EventStore.getEventStore
        SymetricCryptoProvider = conf.SymetricCryptoProvider |> TableStorageSymetricCryptoProvider.create
    }