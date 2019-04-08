module Yobo.Client.Admin.Lessons.Domain

open System
open Yobo.Shared.Communication
open Yobo.Shared.Domain

type AddLessonForm = {
    StartTime : string
    EndTime : string
    Name : string
    Description : string
}
with
    static member Init = {
        StartTime = ""
        EndTime = ""
        Name = ""
        Description = ""
    }

type State = {
    Lessons : Lesson list
    Workshops : Workshop list
    WeekOffset : int
    SelectedDates : DateTimeOffset list
    FormOpened : bool
    AddLessonForm : AddLessonForm
}
with
    static member Init = {
        Lessons = []
        Workshops = []
        WeekOffset = 0
        SelectedDates = []
        FormOpened = false
        AddLessonForm = AddLessonForm.Init
    }

type Msg =
    | Init
    | LoadLessons
    | LessonsLoaded of ServerResult<Lesson list>
    | LoadWorkshops
    | WorkshopsLoaded of ServerResult<Workshop list>
    | WeekOffsetChanged of int
    | DateSelected of DateTimeOffset
    | DateUnselected of DateTimeOffset
    | AddLessonFormOpened of bool
    | AddLessonFormChanged of AddLessonForm
    | SubmitAddLessonForm
    | SubmitAddWorkshopForm
    | AddLessonFormSubmitted of ServerResult<unit>
    | CancelLesson of Guid
    | DeleteWorkshop of Guid
    | LessonCancelled of ServerResult<unit>
    | WorkshopDeleted of ServerResult<unit>