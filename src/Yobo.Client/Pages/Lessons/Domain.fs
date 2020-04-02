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
    | OnlinesForm

type Model = {
    Lessons : Lesson list
    Workshops : Workshop list
    Onlines : OnlineLesson list
    WeekOffset : int
    SelectedDates : DateTimeOffset list
    FormShown : bool
    ActiveForm : ActiveForm
    LessonsForm : ValidatedForm<Request.CreateLessons>
    WorkshopsForm : ValidatedForm<Request.CreateWorkshops>
    OnlinesForm : ValidatedForm<Request.CreateOnlineLessons>
}

module Model =
    let init =
        {
            Lessons = []
            Workshops = []
            Onlines = []
            WeekOffset = 0
            SelectedDates = []
            FormShown = false
            ActiveForm = LessonsForm
            LessonsForm = Request.CreateLessons.init |> ValidatedForm.init
            WorkshopsForm = Request.CreateWorkshops.init |> ValidatedForm.init
            OnlinesForm = Request.CreateOnlineLessons.init |> ValidatedForm.init
        }

type Msg =
    | Init
    | SwitchActiveForm of ActiveForm
    | ShowForm of bool
    | ToggleDate of DateTimeOffset
    | LoadLessons
    | LoadOnlineLessons
    | LoadWorkshops
    | LessonsLoaded of ServerResult<Lesson list>
    | OnlineLessonsLoaded of ServerResult<OnlineLesson list>
    | WorkshopsLoaded of ServerResult<Workshop list>
    | WeekOffsetChanged of int
    | LessonsFormChanged of Request.CreateLessons
    | OnlineLessonsFormChanged of Request.CreateOnlineLessons
    | WorkshopsFormChanged of Request.CreateWorkshops
    | CreateLessons
    | CreateWorkshops
    | CreateOnlineLessons
    | LessonsCreated of ServerResult<unit>
    | WorkshopsCreated of ServerResult<unit>
    | OnlineLessonsCreated of ServerResult<unit>
    
