module Yobo.Client.Pages.Lessons.Domain

open System
open Yobo.Client.Forms
open Yobo.Shared.Core.Admin.Communication
open Yobo.Shared.DateTime
open Yobo.Shared.Errors
open Yobo.Shared.Core.Admin.Domain.Queries

type ActiveForm =
    | LessonsForm

type Model = {
    Lessons : Lesson list
    LessonsLoading : bool
    WeekOffset : int
    SelectedDates : DateTimeOffset list
    FormShown : bool
    ActiveForm : ActiveForm
    LessonsForm : ValidatedForm<Request.CreateLessons>
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
        }

type Msg =
    | LoadLessons
    | LessonsLoaded of ServerResult<Lesson list>
    | ToggleDate of DateTimeOffset
    | WeekOffsetChanged of int
    | ShowForm of bool
    | LessonsFormChanged of Request.CreateLessons
    