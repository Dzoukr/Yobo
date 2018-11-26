module Yobo.API.Configuration

open System
open Microsoft.Extensions.Configuration

let private conf = 
    (ConfigurationBuilder())
        .AddJsonFile(IO.Directory.GetCurrentDirectory() + "\config.development.json", true)
        .AddEnvironmentVariables()
        .Build()

module SymetricCryptoProvide =
    open Yobo.Libraries.Security.TableStorageSymetricCryptoProvider

    let get : Configuration = 
        let isDev = 
            match conf.["crypto:useLocalEmulator"] |> Boolean.TryParse with
            | true, v -> v
            | false, _ -> false
        {
            TableName = conf.["crypto:tableName"]
            Account = if isDev then StorageAccount.LocalEmulator else StorageAccount.Cloud(conf.["crypto:accountName"], conf.["crypto:authKey"])
        }

module EventStore =
    open CosmoStore.TableStorage

    let get : Configuration =
        let isDev = 
            match conf.["eventStore:useLocalEmulator"] |> Boolean.TryParse with
            | true, v -> v
            | false, _ -> false
        if isDev then Configuration.CreateDefaultForLocalEmulator() 
        else Configuration.CreateDefault conf.["eventStore:accountName"] conf.["eventStore:authKey"]

module ReadDb =
    let get : string = conf.["readDb:connectionString"]

module Emails =
    open Yobo.Libraries.Emails

    module Mailjet =
        open Yobo.Libraries.Emails.MailjetProvider

        let get : Configuration = {
            ApiKey = conf.["emails:mailjet:apiKey"]
            SecretKey = conf.["emails:mailjet:secretKey"]
        }

    let from : Address = {
        Name = conf.["emails:from:name"]
        Email = conf.["emails:from:email"]
    }

module Server =
    let baseUrl = Uri(conf.["server:baseUrl"])