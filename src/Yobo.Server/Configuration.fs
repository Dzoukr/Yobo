module Yobo.Server.Configuration

open System
open Microsoft.Extensions.Configuration

type JwtConfiguration = {
    Issuer : string
    Audience : string
    Secret : string
    TokenLifetime : TimeSpan
}

type Configuration = {
    DbConnectionString : string
    JwtConfiguration : JwtConfiguration
}

let load (cfg:IConfigurationRoot) =
    {
        DbConnectionString = cfg.["ReadDbConnectionString"]
        JwtConfiguration = {
            Issuer = cfg.["AuthIssuer"]
            Audience = cfg.["AuthAudience"]
            Secret = cfg.["AuthSecret"]
            TokenLifetime = cfg.["AuthTokenLifetime"] |> TimeSpan.Parse
        }
    }