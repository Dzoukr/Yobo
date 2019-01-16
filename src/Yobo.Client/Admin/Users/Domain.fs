module Yobo.Client.Admin.Users.Domain

open System
open Yobo.Shared.Communication
open Yobo.Shared.Admin.Domain

type State = {
    Users : User list
    UsersLoading : bool
    SelectedUserId : Guid option
    ExpirationDate : DateTime option
    Credits : int
}
with
    static member Init = {
        Users = []
        UsersLoading = false
        SelectedUserId = None
        ExpirationDate = DateTime.UtcNow.AddMonths 4 |> Some
        Credits = 10
    }

type Msg =
    | Init
    | CalendarChanged of DateTime option
    | CreditsChanged of int
    | SubmitForm
    | FormSubmitted of Result<unit, ServerError>
    | LoadUsers
    | UsersLoaded of Result<User list, ServerError>
    | ToggleAddCreditsForm of Guid