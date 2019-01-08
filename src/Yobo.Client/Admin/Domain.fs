module Yobo.Client.Admin.Domain

open System
open Yobo.Shared.Auth.Domain
open Yobo.Shared.Communication
open Yobo.Shared.Domain
open Yobo.Shared.Admin.Domain

type AddCreditsForm = {
    SelectedUserId : Guid option
    ExpirationDate : DateTime option
    Credits : int
}
with
    static member Init = {
        SelectedUserId = None
        ExpirationDate = None
        Credits = 10
    }

type State = {
    Users : User list
    AddCreditsForm : AddCreditsForm
}
with
    static member Init = {
        Users = []
        AddCreditsForm = AddCreditsForm.Init
    }

type Msg =
    | Init
    | ToggleAddCreditsForm of Guid
    | LoadUsers
    | UsersLoaded of Result<User list, ServerError>
    | CalendarChanged of DateTime option