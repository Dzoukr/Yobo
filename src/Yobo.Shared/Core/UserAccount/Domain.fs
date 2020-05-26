module Yobo.Shared.Core.UserAccount.Domain

open System
open Yobo.Shared.Core.Domain

module Queries =
    type UserAccount = {
        Id : Guid
        Email : string
        FirstName : string
        LastName : string
        IsActivated : bool
        Credits : int
        CreditsExpiration : DateTimeOffset option
        IsAdmin : bool
    }
    
    type Lesson = {
        Id : Guid
        StartDate : DateTimeOffset
        EndDate : DateTimeOffset
        Name : string
        Description : string
        Payment : Queries.LessonPayment
    }