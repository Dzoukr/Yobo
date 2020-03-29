module Yobo.Client.Pages.Users.Domain

open System
open Yobo.Client.Forms
open Yobo.Shared.Core.Admin.Communication
open Yobo.Shared.DateTime
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

module Model =
    let init =
        {
            Users = []
            UsersLoading = false
            AddCreditsSelectedUser = None
            AddCreditsForm = Request.AddCredits.init |> ValidatedForm.init
            SetExpirationSelectedUser = None
            SetExpirationForm = Request.SetExpiration.init |> ValidatedForm.init
        }

type Msg =
    | LoadUsers
    | UsersLoaded of ServerResult<User list>
    | ShowAddCreditsForm of Guid option
    | AddCreditsFormChanged of Request.AddCredits
    | AddCreditsFormDateChanged of DateTimeOffset
    | AddCredits
    | CreditsAdded of ServerResult<unit>
    | ShowSetExpirationForm of Guid option
    | SetExpirationFormChanged of Request.SetExpiration
    | SetExpirationFormDateChanged of DateTimeOffset
    | SetExpiration
    | ExpirationSet of ServerResult<unit>
