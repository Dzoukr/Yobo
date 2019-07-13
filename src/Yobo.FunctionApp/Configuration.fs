module Yobo.FunctionApp.Configuration

open System
open Microsoft.Extensions.Configuration

type EmailConfiguration = {
    Mailjet : Yobo.Libraries.Emails.MailjetProvider.Configuration
    From : Yobo.Libraries.Emails.Address
}

type ServerConfiguration = {
    BaseUrl : Uri
}

type AdminConfiguration = {
    Email : string
    Password : string
}

type ApplicationConfiguration = {
    SymetricCryptoProvider : Yobo.Libraries.Security.TableStorageSymetricCryptoProvider.Configuration
    EventStore : CosmoStore.TableStorage.Configuration
    ReadDbConnectionString : string
    Emails : EmailConfiguration
    Server : ServerConfiguration
    Authorization : Yobo.Libraries.Authorization.Configuration
    Admin : AdminConfiguration
}

module private SymetricCryptoProvider =
    open Yobo.Libraries.Security.TableStorageSymetricCryptoProvider

    let get (conf:IConfigurationRoot) : Configuration = 
        let isDev = 
            match conf.["CryptoUseLocalEmulator"] |> Boolean.TryParse with
            | true, v -> v
            | false, _ -> false
        {
            TableName = conf.["CryptoTableName"]
            Account = if isDev then StorageAccount.LocalEmulator else StorageAccount.Cloud(conf.["CryptoAccountName"], conf.["CryptoAuthKey"])
        }

module private EventStore =
    open CosmoStore.TableStorage

    let get (conf:IConfigurationRoot) : Configuration =
        let isDev = 
            match conf.["EventStoreUseLocalEmulator"] |> Boolean.TryParse with
            | true, v -> v
            | false, _ -> false
        if isDev then Configuration.CreateDefaultForLocalEmulator() 
        else Configuration.CreateDefault conf.["EventStoreAccountName"] conf.["EventStoreAuthKey"]

module private Emails =
    open Yobo.Libraries.Emails

    let get (conf:IConfigurationRoot) : EmailConfiguration = {
        Mailjet =  {
            ApiKey = conf.["MailjetApiKey"]
            SecretKey = conf.["MailjetSecretKey"]
        }
        
        From = {
            Name = conf.["EmailsFromName"]
            Email = conf.["EmailsFromEmail"]
        }
    }

module private Server =
    let get (conf:IConfigurationRoot) = {
        BaseUrl = Uri(conf.["ServerBaseUrl"])
    }

module private Authorization =
    open Yobo.Libraries.Authorization

    let get (conf:IConfigurationRoot) : Configuration = {
        Issuer = conf.["AuthIssuer"]
        Audience = conf.["AuthAudience"]
        Secret = conf.["AuthSecret"] |> Base64String.fromString
        TokenLifetime = conf.["AuthTokenLifetime"] |> TimeSpan.Parse
    }

module Admin =
    let get (conf:IConfigurationRoot) = {
        Email = conf.["AdminEmail"]
        Password = conf.["AdminPassword"]
    }

let load (cfg:IConfigurationRoot) =
    {
        SymetricCryptoProvider = SymetricCryptoProvider.get cfg
        EventStore = EventStore.get cfg
        ReadDbConnectionString = cfg.["ReadDbConnectionString"]
        Emails = Emails.get cfg
        Server = Server.get cfg
        Authorization = Authorization.get cfg
        Admin = Admin.get cfg
    }