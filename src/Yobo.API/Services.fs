module Yobo.API.Services
open Yobo.Libraries.Security
open Yobo.Core.Users
open Yobo.Libraries.Authorization

type UsersServices = {
    ReadQueries : ReadQueries.UserQueries
    Authenticator : Authenticator.Authenticator
    Authorizator : Authorizator
}

let createUsersService (conf:Configuration) (connString:string) = {
    ReadQueries = connString |> ReadQueries.createDefault
    Authenticator = Password.verifyPassword |> Authenticator.createDefault connString
    Authorizator = conf |> Jwt.createAuthorizator
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

type Services = {
    Users : UsersServices
    Lessons : LessonsServices
    Workshops : WorkshopsServices
    Emails : Yobo.Libraries.Emails.EmailProvider
    EventStore : CosmoStore.EventStore
    SymetricCryptoProvider : SymetricCryptoProvider.SymetricCryptoProvider
}

let createServices (conf:Configuration.ApplicationConfiguration) : Services =
    {
        Users = createUsersService conf.Authorization conf.ReadDbConnectionString
        Lessons = createLessonsService conf.ReadDbConnectionString
        Workshops = createWorkshopsService conf.ReadDbConnectionString
        Emails = conf.Emails.Mailjet |> MailjetProvider.create
        EventStore = conf.EventStore |> CosmoStore.TableStorage.EventStore.getEventStore
        SymetricCryptoProvider = conf.SymetricCryptoProvider |> TableStorageSymetricCryptoProvider.create
    }