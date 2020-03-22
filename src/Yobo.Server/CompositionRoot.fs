namespace Yobo.Server

open Microsoft.Azure.WebJobs.Description
open System
open System.Security.Claims
open System.Threading.Tasks
open Yobo.Server.Auth
open Yobo.Server.UserAccount
open Yobo.Shared.Domain

type AuthQueries = {
    TryGetUserByEmail : string -> Task<Auth.Domain.Queries.AuthUserView option>
    TryGetUserById : Guid -> Task<Auth.Domain.Queries.BasicUserView option>
}

type AuthCommandHandler = {
    Register : Domain.CmdArgs.Register -> Task<unit>
    ActivateAccount : Domain.CmdArgs.Activate -> Task<unit>
    ForgottenPassword : Domain.CmdArgs.InitiatePasswordReset -> Task<unit>
    ResetPassword : Domain.CmdArgs.CompleteResetPassword -> Task<unit>
}

type AuthRoot = {
    CreateToken : Claim list -> string
    ValidateToken : string -> Claim list option
    VerifyPassword : string -> string -> bool
    CreatePasswordHash : string -> string
    Queries : AuthQueries
    CommandHandler : AuthCommandHandler
}

type UserAccountQueries = {
    GetUserInfo : Guid -> Task<ServerResult<UserAccount.Domain.Queries.UserAccount>>
}

type UserAccountRoot = {
    Queries : UserAccountQueries
}
type CompositionRoot = {
    Auth : AuthRoot
    //UserAccount : UserAccountRoot
}    

module Attributes =
    [<Binding>]
    [<AttributeUsage(AttributeTargets.Parameter)>]
    type CompositionRootAttribute() = inherit Attribute()