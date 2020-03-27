module Yobo.Client.Pages.MyAccount.Domain

open Yobo.Shared.Auth.Communication
open Yobo.Shared.Domain
open Yobo.Shared.Validation
open Yobo.Client.Forms

type Model = {
    //Lessons : Lesson list
    LoggedUser : Yobo.Shared.UserAccount.Domain.Queries.UserAccount
}

module Model =
    let init user = {
        //IsLoading = false
        LoggedUser = user
    }

type Msg =
    | LoadMyLessons
    //| MyLessonsLoaded of ServerResult<Lesson list>