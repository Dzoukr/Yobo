module Yobo.Server.Auth.HttpHandlers

open System
open System.Data
open System.Security.Claims
open System.Threading.Tasks
open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Giraffe
open Yobo.Shared.Auth.Communication
open FSharp.Control.Tasks
open Microsoft.Data.SqlClient
open Yobo.Libraries.Authentication
open Yobo.Server
open Yobo.Server.Auth.Database.Queries
open Yobo.Shared.Auth.Validation
open Yobo.Server.Configuration
open FSharp.Rop
open Yobo.Shared.Communication

let userToClaims (u:AuthUserView) =
    seq [
        Claim("Id", u.Id.ToString())
        Claim("Email", u.Email)
        Claim("FirstName", u.FirstName)
        Claim("LastName", u.LastName)
    ]

let private login (connString:string) (jwt:JwtConfiguration) (l:Request.Login) =
    task {
        use conn = new SqlConnection(connString)
        let! user = Database.Queries.tryGetUserByEmail conn l.Email
        return
            user
            |> Option.map (fun x -> x, Password.verifyPassword l.Password x.PasswordHash)
            |> Option.bind (fun (user,isVerified) -> if isVerified then Some user else None)
            |> Option.map (userToClaims >> Jwt.createJwtToken jwt.Audience jwt.Issuer jwt.Secret jwt.TokenLifetime)
            |> Option.map (fun x -> x.Token)
            |> Result.ofOption (AuthenticationError.InvalidLoginOrPassword |> ServerError.Authentication)
    }

let private authService (connString:string) (jwt:JwtConfiguration) : AuthService = {
    GetToken = Flow.ofTaskResultValidated validateLogin (login connString jwt) >> Async.AwaitTask
}

let authServiceHandler (cfg:Configuration) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder AuthService.RouteBuilder
    |> Remoting.fromValue (authService cfg.DbConnectionString cfg.JwtConfiguration)
    |> Remoting.buildHttpHandler