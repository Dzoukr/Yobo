module Yobo.Client.Pages.Lessons.Domain

open System
open Yobo.Client.Forms
open Yobo.Shared.Core.Admin.Communication
open Yobo.Shared.DateTime
open Yobo.Shared.Errors
open Yobo.Shared.Core.Admin.Domain.Queries

type ActiveForm =
    | LessonsForm
    | WorkshopsForm

type Model = {
    Lessons : Lesson list
    LessonsLoading : bool
    WeekOffset : int
    SelectedDates : DateTimeOffset list
    FormShown : bool
    ActiveForm : ActiveForm
    LessonsForm : ValidatedForm<Request.CreateLessons>
    WorkshopsForm : ValidatedForm<Request.CreateWorkshops>
}

module Model =
    let init =
        {
            Lessons = []
            LessonsLoading = false
            WeekOffset = 0
            SelectedDates = []
            FormShown = false
            ActiveForm = LessonsForm
            LessonsForm = Request.CreateLessons.init |> ValidatedForm.init
            WorkshopsForm = Request.CreateWorkshops.init |> ValidatedForm.init
        }

type Msg =
    | Init
    | SwitchActiveForm of ActiveForm
    | LoadLessons
    | LessonsLoaded of ServerResult<Lesson list>
    | ToggleDate of DateTimeOffset
    | WeekOffsetChanged of int
    | ShowForm of bool
    | LessonsFormChanged of Request.CreateLessons
    | CreateLessons
    | LessonsCreated of ServerResult<unit>
    | WorkshopsFormChanged of Request.CreateWorkshops
    | CreateWorkshops
    