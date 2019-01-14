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
        ExpirationDate = DateTime.UtcNow.AddMonths 4 |> Some
        Credits = 10
    }

type LessonsState = {
    WeekOffset : int
}
with
    static member Init = {
        WeekOffset = 0
   }

type State = {
    Users : User list
    AddCreditsForm : AddCreditsForm
    Lessons : LessonsState
}
with
    static member Init = {
        Users = []
        AddCreditsForm = AddCreditsForm.Init
        Lessons = LessonsState.Init
    }

type AddCreditsFormMsg =
    | CalendarChanged of DateTime option
    | CreditsChanged of int
    | SubmitForm
    | FormSubmitted of Result<unit, ServerError>

type LessonsMsg =
    | WeekOffsetChanged of int

type Msg =
    | Init
    | LoadUsers
    | UsersLoaded of Result<User list, ServerError>
    | ToggleAddCreditsForm of Guid
    | AddCreditsFormMsg of AddCreditsFormMsg
    | LessonsMsg of LessonsMsg