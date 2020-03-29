module Yobo.Client.Pages.Users.Domain

open System
open Yobo.Client.Forms
open Yobo.Shared.DateTime
open Yobo.Shared.Errors
open Yobo.Shared.Core.Admin.Domain.Queries

type Model = {
    Users : User list
    UsersLoading : bool
    
    AddCreditsSelectedUser : Guid option    
    
//    AddCreditsForm : ValidatedForm<Request.Login>
//    ExpirationDate : DateTimeOffset option
//    Credits : int
}

module Model =
    let init =
        {
            Users = []
            UsersLoading = false
            AddCreditsSelectedUser = None

//            ExpirationDate = DateTimeOffset.Now.EndOfTheDay().AddMonths 4 |> Some
//            Credits = 10
        }

type Msg =
    | LoadUsers
    | UsersLoaded of ServerResult<User list>
    | ShowAddCreditsForm of Guid option
    
//    | CalendarChanged of DateTimeOffset option
//    | CreditsChanged of int
//    | SubmitForm
//    | FormSubmitted of Result<unit, ServerError>
//    | ToggleAddCreditsForm of Guid