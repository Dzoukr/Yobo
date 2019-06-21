module Yobo.API.Configuration

open System
open Microsoft.Extensions.Configuration

type EmailConfiguration = {
    Mailjet : Yobo.Libraries.Emails.MailjetProvider.Configuration
    From : Yobo.Libraries.Emails.Address
}

type ServerConfiguration = {
    BaseUrl : Uri
    WwwRootPath : string
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
            match conf.["crypto:useLocalEmulator"] |> Boolean.TryParse with
            | true, v -> v
            | false, _ -> false
        {
            TableName = conf.["crypto:tableName"]
            Account = if isDev then StorageAccount.LocalEmulator else StorageAccount.Cloud(conf.["crypto:accountName"], conf.["crypto:authKey"])
        }

module private EventStore =
    open CosmoStore.TableStorage

    let get (conf:IConfigurationRoot) : Configuration =
        let isDev = 
            match conf.["eventStore:useLocalEmulator"] |> Boolean.TryParse with
            | true, v -> v
            | false, _ -> false
        if isDev then Configuration.CreateDefaultForLocalEmulator() 
        else Configuration.CreateDefault conf.["eventStore:accountName"] conf.["eventStore:authKey"]

module private Emails =
    open Yobo.Libraries.Emails

    let get (conf:IConfigurationRoot) : EmailConfiguration = {
        Mailjet =  {
            ApiKey = conf.["emails:mailjet:apiKey"]
            SecretKey = conf.["emails:mailjet:secretKey"]
        }
        
        From = {
            Name = conf.["emails:from:name"]
            Email = conf.["emails:from:email"]
        }
    }

module private Server =
    let get (conf:IConfigurationRoot) = {
        BaseUrl = Uri(conf.["server:baseUrl"])
        WwwRootPath = "./wwwroot"
    }

module private Authorization =
    open Yobo.Libraries.Authorization

    let get (conf:IConfigurationRoot) : Configuration = {
        Issuer = conf.["auth:issuer"]
        Audience = conf.["auth:audience"]
        Secret = conf.["auth:secret"] |> Base64String.fromString
        TokenLifetime = conf.["auth:tokenLifetime"] |> TimeSpan.Parse
    }

module Admin =
    let get (conf:IConfigurationRoot) = {
        Email = conf.["admin:email"]
        Password = conf.["admin:password"]
    }

let load (cfg:IConfigurationRoot) =
    {
        SymetricCryptoProvider = SymetricCryptoProvider.get cfg
        EventStore = EventStore.get cfg
        ReadDbConnectionString = cfg.["readDb:connectionString"]
        Emails = Emails.get cfg
        Server = Server.get cfg
        Authorization = Authorization.get cfg
        Admin = Admin.get cfg
    }