module Yobo.Client.Admin.Lessons.Domain

open System
open Yobo.Shared.Communication
open Yobo.Shared.Domain

type State = {
    Lessons : Lesson list
    WeekOffset : int
    SelectedDates : DateTimeOffset list
    StartTime : string
    EndTime : string
    Name : string
    Description : string
    FormOpened : bool
}
with
    static member Init = {
        Lessons = []
        WeekOffset = 0
        SelectedDates = []
        StartTime = ""
        EndTime = ""
        Name = ""
        Description = ""
        FormOpened = false
    }

type Msg =
    | Init
    | LoadLessons
    | LessonsLoaded of ServerResult<Lesson list>
    | WeekOffsetChanged of int
    | DateSelected of DateTimeOffset
    | DateUnselected of DateTimeOffset
    | StartChanged of string
    | FormOpened of bool
    | EndChanged of string
    | NameChanged of string
    | DescriptionChanged of string
    | SubmitLessonsForm
    | LessonsFormSubmitted of ServerResult<unit>
    | CancelLesson of Guid
    | LessonCancelled of ServerResult<unit>