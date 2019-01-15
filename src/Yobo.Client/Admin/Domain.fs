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

type AddLessonsForm = {
    WeekOffset : int
    SelectedDates : DateTime list
    StartTime : string
    EndTime : string
    Name : string
    Description : string
    FormOpened : bool
}
with
    static member Init = {
        WeekOffset = 0
        SelectedDates = []
        StartTime = ""
        EndTime = ""
        Name = ""
        Description = ""
        FormOpened = false
   }

type State = {
    Users : User list
    Lessons : Lesson list
    AddCreditsForm : AddCreditsForm
    AddLessonsForm : AddLessonsForm
}
with
    static member Init = {
        Users = []
        Lessons = []
        AddCreditsForm = AddCreditsForm.Init
        AddLessonsForm = AddLessonsForm.Init
    }

type AddCreditsFormMsg =
    | CalendarChanged of DateTime option
    | CreditsChanged of int
    | SubmitForm
    | FormSubmitted of Result<unit, ServerError>

type LessonsMsg =
    | WeekOffsetChanged of int
    | DateSelected of DateTime
    | DateUnselected of DateTime
    | StartChanged of string
    | FormOpened of bool
    | EndChanged of string
    | NameChanged of string
    | DescriptionChanged of string
    | SubmitLessonsForm
    | LessonsFormSubmitted of Result<unit, ServerError>

type Msg =
    | Init
    | LoadUsers
    | LoadLessons
    | UsersLoaded of Result<User list, ServerError>
    | LessonsLoaded of Result<Lesson list, ServerError>
    | ToggleAddCreditsForm of Guid
    | AddCreditsFormMsg of AddCreditsFormMsg
    | LessonsMsg of LessonsMsg