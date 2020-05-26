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
   

type Model =
    {
        Lessons : Lesson list
        LoggedUser : Yobo.Shared.Core.UserAccount.Domain.Queries.UserAccount
    }
    interface IUserAwareModel with
        member x.UpdateUser (u:UserAccount) = { x with LoggedUser = u } :> IUserAwareModel

module Model =
    let init user =
        {
            LoggedUser = user
            Lessons = []
        }
        
    let sharedLessons (m:Model) =
        (m.Lessons |> List.map SharedLesson.fromLesson)
        |> List.sortBy (fun x -> x.StartDate)

type Msg =
    | Init
    | LoadMyLessons
    | MyLessonsLoaded of ServerResult<Lesson list>