module Yobo.Client.Pages.MyAccount.Domain

open Yobo.Shared.Auth.Communication
open Yobo.Shared.Errors
open Yobo.Shared.Validation
open Yobo.Client.Forms
open Yobo.Client.Interfaces
open Yobo.Shared.Core.UserAccount.Domain.Queries

type Model =
    {
        //Lessons : Lesson list
        LoggedUser : Yobo.Shared.Core.UserAccount.Domain.Queries.UserAccount
    }
    interface IUserAwareModel with
        member x.UpdateUser (u:UserAccount) = { x with LoggedUser = u } :> IUserAwareModel

module Model =
    let init user =
        {
            LoggedUser = user
        }

type Msg =
    | LoadMyLessons
    //| MyLessonsLoaded of ServerResult<Lesson list>