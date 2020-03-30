module Yobo.Client.Pages.Lessons.Domain

open System
open Yobo.Client.Forms
open Yobo.Shared.Core.Admin.Communication
open Yobo.Shared.DateTime
open Yobo.Shared.Errors
open Yobo.Shared.Core.Admin.Domain.Queries

type Model = {
    Lessons : Lesson list
    LessonsLoading : bool
}

module Model =
    let init =
        {
            Lessons = []
            LessonsLoading = false
        }

type Msg =
    | LoadLessons
    | LessonsLoaded of ServerResult<Lesson list>
    