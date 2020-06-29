module Yobo.Client.Pages.Users.Domain

open System
open Yobo.Client.Forms
open Yobo.Shared.Core.Admin.Communication
open Yobo.Shared.Errors
open Yobo.Shared.Core.Admin.Domain.Queries

type Model = {
    Users : User list
    UsersLoading : bool
    AddCreditsSelectedUser : Guid option    
    AddCreditsForm : ValidatedForm<Request.AddCredits>
    SetExpirationSelectedUser : Guid option    
    SetExpirationForm : ValidatedForm<Request.SetExpiration>
}

type Msg =
    | LoadUsers
    | UsersLoaded of ServerResult<User list>
    | ShowAddCreditsForm of Guid option
    | AddCreditsFormChanged of Request.AddCredits
    | AddCredits
    | CreditsAdded of ServerResult<unit>
    | ShowSetExpirationForm of Guid option
    | SetExpirationFormChanged of Request.SetExpiration
    | SetExpiration
    | ExpirationSet of ServerResult<unit>
