namespace Yobo.Server

open Microsoft.Azure.WebJobs.Description
open System
open System.Security.Claims
open System.Threading.Tasks

type AuthQueries = {
    TryGetUserByEmail : string -> Task<Auth.Domain.Queries.AuthUserView option>
    TryGetUserById : Guid -> Task<Auth.Domain.Queries.BasicUserView option>
}

type AuthCommandHandler = {
    Register : Auth.Domain.CmdArgs.Register -> Task<unit>
    ActivateAccount : Auth.Domain.CmdArgs.Activate -> Task<unit>
    ForgottenPassword : Auth.Domain.CmdArgs.InitiatePasswordReset -> Task<unit>
    ResetPassword : Auth.Domain.CmdArgs.CompleteResetPassword -> Task<unit>
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
    TryGetUserInfo : Guid -> Task<Yobo.Shared.Core.UserAccount.Domain.Queries.UserAccount option>
}

type UserAccountRoot = {
    Queries : UserAccountQueries
}

type AdminQueries = {
    GetAllUsers : unit -> Task<Yobo.Shared.Core.Admin.Domain.Queries.User list>
    GetLessons : DateTimeOffset * DateTimeOffset -> Task<Yobo.Shared.Core.Admin.Domain.Queries.Lesson list>
}

type AdminCommandHandler = {
    AddCredits : Core.Domain.CmdArgs.AddCredits -> Task<unit>
    SetExpiration : Core.Domain.CmdArgs.SetExpiration -> Task<unit>
    CreateLesson : Core.Domain.CmdArgs.CreateLesson -> Task<unit>
}

type AdminRoot = {
    Queries : AdminQueries
    CommandHandler : AdminCommandHandler
}

type CompositionRoot = {
    Auth : AuthRoot
    UserAccount : UserAccountRoot
    Admin : AdminRoot
}    

module Attributes =
    [<Binding>]
    [<AttributeUsage(AttributeTargets.Parameter)>]
    type CompositionRootAttribute() = inherit Attribute()