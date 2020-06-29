module Yobo.Client.Pages.MyAccount.Domain

open Yobo.Shared.Errors
open Yobo.Shared.Core.UserAccount.Domain.Queries
  

type Model =
    {
        Lessons : Lesson list
        LoggedUser : Yobo.Shared.Core.UserAccount.Domain.Queries.UserAccount
    }
 
type Msg =
    | LoadMyLessons
    | MyLessonsLoaded of ServerResult<Lesson list>