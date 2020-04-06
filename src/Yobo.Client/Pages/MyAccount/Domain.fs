module Yobo.Client.Pages.MyAccount.Domain

open System
open Yobo.Shared.Auth.Communication
open Yobo.Shared.Errors
open Yobo.Shared.Validation
open Yobo.Client.Forms
open Yobo.Client.Interfaces
open Yobo.Shared.Core.Domain.Queries
open Yobo.Shared.Core.UserAccount.Domain.Queries

type SharedLesson = {
    Id : Guid
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    Name : string
    Description : string
    Payment : LessonPayment
    IsOnline : bool
}

module SharedLesson =
    let fromLesson (l:Lesson) =
        {
            Id = l.Id
            StartDate = l.StartDate
            EndDate = l.EndDate
            Name = l.Name
            Description = l.Description
            Payment = l.Payment
            IsOnline = false
        }
    
    let fromOnlineLesson (l:OnlineLesson) =
        {
            Id = l.Id
            StartDate = l.StartDate
            EndDate = l.EndDate
            Name = l.Name
            Description = l.Description
            Payment = l.Payment
            IsOnline = true
        }        

type Model =
    {
        Lessons : Lesson list
        OnlineLessons : OnlineLesson list
        LoggedUser : Yobo.Shared.Core.UserAccount.Domain.Queries.UserAccount
    }
    interface IUserAwareModel with
        member x.UpdateUser (u:UserAccount) = { x with LoggedUser = u } :> IUserAwareModel

module Model =
    let init user =
        {
            LoggedUser = user
            Lessons = []
            OnlineLessons = []
        }
        
    let sharedLessons (m:Model) =
        (m.Lessons |> List.map SharedLesson.fromLesson)
        @
        (m.OnlineLessons |> List.map SharedLesson.fromOnlineLesson)
        |> List.sortBy (fun x -> x.StartDate)

type Msg =
    | Init
    | LoadMyLessons
    | MyLessonsLoaded of ServerResult<Lesson list>
    | LoadMyOnlineLessons
    | MyOnlineLessonsLoaded of ServerResult<OnlineLesson list>