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
open Microsoft.AspNetCore.Http
open Yobo.Server
open Yobo.Shared.Domain

let private userToClaims (u:Domain.Queries.AuthUserView) =
    [
        Claim("Id", u.Id.ToString())
        Claim("Email", u.Email)
        Claim("FirstName", u.FirstName)
        Claim("LastName", u.LastName)
        Claim("IsAdmin", string u.IsAdmin)
    ]

let private login (authRoot:AuthRoot) (l:Request.Login) =
    task {
        let! user = authRoot.Queries.TryGetUserByEmail l.Email
        return
            user
            |> Option.map (fun x -> x, authRoot.VerifyPassword l.Password x.PasswordHash)
            |> Option.bind (fun (user,isVerified) -> if isVerified then Some user else None)
            |> Option.map (userToClaims >> authRoot.CreateToken)
            |> ServerError.ofOption (AuthenticationError.InvalidLoginOrPassword |> ServerError.Authentication)
    }
    
let private refreshToken (authRoot:AuthRoot) (token:string) =
    token
    |> authRoot.ValidateToken
    |> Option.map (authRoot.CreateToken)
    |> ServerError.ofOption (AuthenticationError.InvalidOrExpiredToken |> ServerError.Authentication)

let private register (authRoot:AuthRoot) (r:Request.Register) =
    task {
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
        return! authRoot.CommandHandler.Register args            
    }

let private activateAccount (authRoot:AuthRoot) (key:Guid) =
    task {
        let args = { ActivationKey = key } : CmdArgs.Activate
        return! authRoot.CommandHandler.ActivateAccount args
    }
    
let private forgottenPassword (authRoot:AuthRoot) (r:Request.ForgottenPassword) =
    task {
        let args : CmdArgs.InitiatePasswordReset =
            {
                Email = r.Email
                PasswordResetKey = Guid.NewGuid()
            }
        return! authRoot.CommandHandler.ForgottenPassword args            
    }

let private resetPassword (authRoot:AuthRoot) (r:Request.ResetPassword) =
    task {
        let args : CmdArgs.CompleteResetPassword =
            {
                PasswordResetKey = r.PasswordResetKey
                PasswordHash = authRoot.CreatePasswordHash r.Password
            }
        return! authRoot.CommandHandler.ResetPassword args            
    }

let private authService (root:AuthRoot) : AuthService =
    {
        GetToken = ServerError.validate validateLogin >> (login root) >> Async.AwaitTask
        RefreshToken = (refreshToken root) >> async.Return
        Register = ServerError.validate validateRegister >> (register root) >> Async.AwaitTask
        ActivateAccount = (activateAccount root) >> Async.AwaitTask
        ForgottenPassword = ServerError.validate validateForgottenPassword >> (forgottenPassword root) >> Async.AwaitTask
        ResetPassword = ServerError.validate validateResetPassword >> (resetPassword root) >> Async.AwaitTask
    }

let authServiceHandler (root:CompositionRoot) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder AuthService.RouteBuilder
    |> Remoting.fromValue (authService root.Auth)
    |> Remoting.withErrorHandler Remoting.errorHandler
    |> Remoting.buildHttpHandler
    
module private Bearer =
    open Microsoft.Extensions.Primitives

    let private (|BearerToken|_|) (d:IHeaderDictionary) =
        match d.["authorization"] with
        | x when x = StringValues.Empty -> None
        | x ->
            if x.[0].StartsWith("Bearer ") then x.[0].Replace("Bearer ","") |> Some
            else None

    let tryGetToken (ctx:HttpContext) =
        match ctx.Request.Headers with
        | BearerToken token -> Some token
        | _ -> None

let onlyForLoggedUser (authRoot:AuthRoot) next (ctx:HttpContext) =
    let claims =
        ctx
        |> Bearer.tryGetToken
        |> Option.bind (authRoot.ValidateToken)
    match claims with
    | Some c ->
        ctx.User <- ClaimsPrincipal(ClaimsIdentity(c))
        next ctx
    | None -> RequestErrors.UNAUTHORIZED "Bearer" "Yobo" "Unauthorized" next ctx

let onlyForAdmin next (ctx:HttpContext) =
    let isAdmin =
        ctx.User.Claims
        |> Seq.find (fun x -> x.Type = "IsAdmin")
        |> fun x -> x.Value
        |> Boolean.Parse
    if isAdmin then next ctx else RequestErrors.UNAUTHORIZED "Bearer" "Yobo" "Unauthorized" next ctx

let withUser (proxyBuilder:Guid -> 'proxy) (ctx:HttpContext) =
    let userId =
        ctx.User.Claims
        |> Seq.find (fun x -> x.Type = "Id")
        |> fun x -> x.Value
        |> Guid
    userId |> proxyBuilder    
     