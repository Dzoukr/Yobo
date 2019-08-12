module Yobo.Client.MyLessons.Domain

open Yobo.Shared.MyLessons.Domain
open Yobo.Shared.Communication
open System

type State = {
    Lessons : Lesson list
    LoggedUser : Yobo.Shared.Domain.User option
}
with
    static member Init = {
        Lessons = []
        LoggedUser = None
    }

type Msg =
    | Init
    | LoadMyLessons
    | MyLessonsLoaded of ServerResult<Lesson list>