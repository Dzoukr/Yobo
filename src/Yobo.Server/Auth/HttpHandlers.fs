module Yobo.Server.Auth.HttpHandlers

open System
open System.Security.Claims
open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Giraffe
open Yobo.Shared.Auth.Communication
open FSharp.Control.Tasks
open Yobo.Server.Auth.Domain
open Yobo.Shared.Auth.Validation
open FSharp.Rop.Result
open FSharp.Rop.TaskResult
open Yobo.Server.CompositionRoot
open Yobo.Shared.Domain

let private userToClaims (u:Queries.AuthUserView) =
    [
        Claim("Id", u.Id.ToString())
        Claim("Email", u.Email)
        Claim("FirstName", u.FirstName)
        Claim("LastName", u.LastName)
    ]

let private login (authRoot:AuthRoot) (l:Request.Login) =
    task {
        use conn = authRoot.GetSqlConnection()
        let! user = authRoot.Queries.TryGetUserByEmail conn l.Email
        return
            user
            |> Option.map (fun x -> x, authRoot.VerifyPassword l.Password x.PasswordHash)
            |> Option.bind (fun (user,isVerified) -> if isVerified then Some user else None)
            |> Option.map (userToClaims >> authRoot.CreateToken)
            |> Result.ofOption (AuthenticationError.InvalidLoginOrPassword |> ServerError.Authentication)
    }
    
let private refreshToken (authRoot:AuthRoot) (token:string) =
    token
    |> authRoot.ValidateToken
    |> Option.map (authRoot.CreateToken)
    |> Result.ofOption (AuthenticationError.InvalidOrExpiredToken |> ServerError.Authentication)

let private register (authRoot:AuthRoot) (r:Request.Register) =
    task {
        use conn = authRoot.GetSqlConnection()
        let args : CmdArgs.Register =
            {
                Id = Guid.NewGuid()
                ActivationKey = Guid.NewGuid()
                PasswordHash = authRoot.CreatePasswordHash r.Password
                FirstName = r.FirstName
                LastName = r.LastName
                Email = r.Email.ToLower()
                Newsletters = r.NewslettersButtonChecked
            }
        return! (authRoot.CommandHandler.Register conn args)            
    }

let private authService (root:CompositionRoot) : AuthService =
    {
        GetToken =
            ServerResult.ofValidation validateLogin
            >> TaskResult.ofResult
            >> TaskResult.bind (login root.Auth)
            >> Async.AwaitTask
        RefreshToken =
            (refreshToken root.Auth)
            >> TaskResult.ofResult
            >> Async.AwaitTask
        Register =
            ServerResult.ofValidation validateRegister
            >> TaskResult.ofResult
            >> TaskResult.bind (register root.Auth)
            >> Async.AwaitTask
    }

let authServiceHandler (root:CompositionRoot) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder AuthService.RouteBuilder
    |> Remoting.fromValue (authService root)
    |> Remoting.buildHttpHandler