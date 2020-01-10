module Yobo.Server.CompositionRoot

open System
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.Data.SqlClient
open Microsoft.Extensions.Configuration
open Yobo.Libraries.Authentication
open Yobo.Server.Auth
open Yobo.Shared.Domain

type AuthQueries = {
    TryGetUserByEmail : SqlConnection -> string -> Task<Auth.Domain.Queries.AuthUserView option>
}

type AuthProjections = {
    GetAllUsers : SqlConnection -> Task<Auth.CommandHandler.Projections.ExistingUser list>
}

type AuthCommandHandler = {
    Register : SqlConnection -> Domain.CmdArgs.Register -> Task<Result<Guid,ServerError>>
}

type AuthRoot = {
    GetSqlConnection : unit -> SqlConnection
    CreateToken : Claim list -> string
    ValidateToken : string -> Claim list option
    VerifyPassword : string -> string -> bool
    CreatePasswordHash : string -> string
    Queries : AuthQueries
    CommandHandler : AuthCommandHandler
}

module AuthRoot =
    
    open FSharp.Control.Tasks.V2
    open FSharp.Rop.TaskResult
    
    let compose getSqlConnection (cfg:IConfigurationRoot) =
        // config
        let issuer = cfg.["AuthIssuer"]
        let audience = cfg.["AuthAudience"]
        let secret = cfg.["AuthSecret"]
        let tokenLifetime = cfg.["AuthTokenLifetime"] |> TimeSpan.Parse
        
        let pars = Jwt.getParameters audience issuer secret
        
        let eventHandler = Auth.EventHandler.handle
        
        {
            GetSqlConnection = getSqlConnection
            CreateToken = Jwt.createJwtToken audience issuer secret tokenLifetime >> fun x -> x.Token
            ValidateToken = Jwt.validateToken pars >> Option.map List.ofSeq
            VerifyPassword = Password.verifyPassword
            CreatePasswordHash = Password.createHash
            Queries = {
                TryGetUserByEmail = Auth.Database.Queries.tryGetUserByEmail
            }
            CommandHandler = {
                Register = fun conn args -> task {
                    let! projections = Auth.Database.Projections.getAll conn
                    return!
                        args
                        |> CommandHandler.register projections
                        |> Result.mapError Authentication
                        |> TaskResult.ofTaskAndResult (eventHandler conn)
                        |> TaskResult.map (fun _ -> args.Id)
                }
            }
        } : AuthRoot
    
type CompositionRoot = {
    Auth : AuthRoot
}    

module CompositionRoot =
    let compose (cfg:IConfigurationRoot) =
        Dapper.FSharp.OptionTypes.register()
        let getSqlConnection = fun _ -> new SqlConnection(cfg.["ReadDbConnectionString"])
        
        {
            Auth = AuthRoot.compose getSqlConnection cfg
        } : CompositionRoot