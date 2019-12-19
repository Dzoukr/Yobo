module Yobo.Server.CompositionRoot

open System
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.Data.SqlClient
open Microsoft.Extensions.Configuration
open Yobo.Libraries.Authentication

type AuthQueries = {
    TryGetUserByEmail : SqlConnection -> string -> Task<Auth.Domain.Queries.AuthUserView option>
}

module AuthQueries =
    let compose = {
        TryGetUserByEmail = Auth.Database.Queries.tryGetUserByEmail
    }

type AuthProjections = {
    GetAllUsers : SqlConnection -> Task<Auth.CommandHandler.Projections.ExistingUser list>
}

module AuthProjections =
    let compose = {
        GetAllUsers = Auth.Database.Projections.getAll
    }

type AuthRoot = {
    GetSqlConnection : unit -> SqlConnection
    CreateToken : Claim list -> string
    ValidateToken : string -> Claim list option
    VerifyPassword : string -> string -> bool
    CreatePasswordHash : string -> string
    Queries : AuthQueries
    Projections : AuthProjections
}

module AuthRoot =
    
    let compose getSqlConnection (cfg:IConfigurationRoot) =
        // config
        let issuer = cfg.["AuthIssuer"]
        let audience = cfg.["AuthAudience"]
        let secret = cfg.["AuthSecret"]
        let tokenLifetime = cfg.["AuthTokenLifetime"] |> TimeSpan.Parse
        
        let pars = Jwt.getParameters audience issuer secret
        
        {
            GetSqlConnection = getSqlConnection
            CreateToken = Jwt.createJwtToken audience issuer secret tokenLifetime >> fun x -> x.Token
            ValidateToken = Jwt.validateToken pars >> Option.map List.ofSeq
            VerifyPassword = Password.verifyPassword
            CreatePasswordHash = Password.createHash
            Queries = AuthQueries.compose
            Projections = AuthProjections.compose
        } : AuthRoot
    
type CompositionRoot = {
    Auth : AuthRoot
}    

module CompositionRoot =
    let compose (cfg:IConfigurationRoot) =
        
        let getSqlConnection = fun _ -> new SqlConnection(cfg.["ReadDbConnectionString"])
        
        {
            Auth = AuthRoot.compose getSqlConnection cfg
        } : CompositionRoot