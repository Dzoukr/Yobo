module Yobo.Client.Calendar.Domain

open Yobo.Shared.Calendar.Domain
open Yobo.Shared.Communication

type State = {
    Lessons : Lesson list
    WeekOffset : int
}
with
    static member Init = {
        Lessons = []
        WeekOffset = 0
    }

type Msg =
    | Init
    | LoadUserLessons
    | UserLessonsLoaded of Result<Lesson list, ServerError>
    | WeekOffsetChanged of int