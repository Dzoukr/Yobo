module Yobo.Client.MyLessons.Domain

open Yobo.Shared.Calendar.Domain
open Yobo.Shared.Communication
open System

type State = {
    Lessons : Lesson list
}
with
    static member Init = {
        Lessons = []
    }

type Msg =
    | Init
    | LoadMyLessons
    | MyLessonsLoaded of ServerResult<Lesson list>