module Yobo.Client.Pages.Lessons.Domain

open System
open Yobo.Client.Forms
open Yobo.Shared.Core.Admin.Communication
open Yobo.Shared.Errors
open Yobo.Shared.Core.Admin.Domain.Queries

type ActiveForm =
    | LessonsForm
    | WorkshopsForm
    | OnlinesForm

type ActiveItem =
    | Lesson of Lesson
    | Workshop of Workshop
    | OnlineLesson of OnlineLesson

type Model = {
    Lessons : Lesson list
    Workshops : Workshop list
    Onlines : OnlineLesson list
    WeekOffset : int
    SelectedDates : DateTimeOffset list
    
    ActiveForm : ActiveForm option
    LessonsForm : ValidatedForm<Request.CreateLessons>
    WorkshopsForm : ValidatedForm<Request.CreateWorkshops>
    OnlinesForm : ValidatedForm<Request.CreateOnlineLessons>
    
    ActiveItem : ActiveItem option
}

module Model =
    let init =
        {
            Lessons = []
            Workshops = []
            Onlines = []
            WeekOffset = 0
            SelectedDates = []
            ActiveForm = None
            LessonsForm = Request.CreateLessons.init |> ValidatedForm.init
            WorkshopsForm = Request.CreateWorkshops.init |> ValidatedForm.init
            OnlinesForm = Request.CreateOnlineLessons.init |> ValidatedForm.init
            ActiveItem = None
        }

type Msg =
    | Init
    | SelectActiveForm of ActiveForm option
    | ToggleDate of DateTimeOffset
    | WeekOffsetChanged of int
    | LoadLessons
    | LoadOnlineLessons
    | LoadWorkshops
    | LessonsLoaded of ServerResult<Lesson list>
    | OnlineLessonsLoaded of ServerResult<OnlineLesson list>
    | WorkshopsLoaded of ServerResult<Workshop list>
    | LessonsFormChanged of Request.CreateLessons
    | OnlineLessonsFormChanged of Request.CreateOnlineLessons
    | WorkshopsFormChanged of Request.CreateWorkshops
    | CreateLessons
    | CreateWorkshops
    | CreateOnlineLessons
    | LessonsCreated of ServerResult<unit>
    | WorkshopsCreated of ServerResult<unit>
    | OnlineLessonsCreated of ServerResult<unit>
    | SelectActiveItem of ActiveItem option
    
