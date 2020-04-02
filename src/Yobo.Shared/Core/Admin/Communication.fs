module Yobo.Shared.Core.Admin.Communication

open System
open Domain
    
[<RequireQualifiedAccess>]
module Request =
    type AddCredits = {
        UserId : Guid
        Credits : int
        Expiration : DateTimeOffset
    }
    
    module AddCredits =
        let init = {
            UserId = Guid.Empty
            Credits = 0
            Expiration = DateTimeOffset.MinValue
        }
    
    type SetExpiration = {
        UserId : Guid
        Expiration : DateTimeOffset
    }
    
    module SetExpiration =
        let init = {
            UserId = Guid.Empty
            Expiration = DateTimeOffset.MinValue
        }
        
    type CreateLessons = {
        Dates : DateTimeOffset list
        StartTime : int * int
        EndTime : int * int
        Name : string
        Description : string
        Capacity : int
    }
    
    module CreateLessons =
        let init = {
            Dates = []
            StartTime = 0,0
            EndTime = 0,0
            Name = ""
            Description = ""
            Capacity = 0
        }
        
    type CreateWorkshops = {
        Dates : DateTimeOffset list
        StartTime : int * int
        EndTime : int * int
        Name : string
        Description : string
    }
    
    module CreateWorkshops =
        let init = {
            Dates = []
            StartTime = 0,0
            EndTime = 0,0
            Name = ""
            Description = ""
        }
        
    type CreateOnlineLessons = {
        Dates : DateTimeOffset list
        StartTime : int * int
        EndTime : int * int
        Name : string
        Description : string
        Capacity : int
    }
    
    module CreateOnlineLessons =
        let init = {
            Dates = []
            StartTime = 0,0
            EndTime = 0,0
            Name = ""
            Description = ""
            Capacity = 0
        }
    
    type ChangeLessonDescription = {
        Id : Guid
        Name : string
        Description : string
    }
    
    module ChangeLessonDescription =
        let init = {
            Id = Guid.Empty
            Name = ""
            Description = ""
        }
        
    
type AdminService = {
    GetAllUsers : unit -> Async<Queries.User list>
    AddCredits : Request.AddCredits -> Async<unit>
    SetExpiration : Request.SetExpiration -> Async<unit>
    GetLessons : DateTimeOffset * DateTimeOffset -> Async<Queries.Lesson list>
    CreateLessons : Request.CreateLessons -> Async<unit>
    GetWorkshops : DateTimeOffset * DateTimeOffset -> Async<Queries.Workshop list>
    CreateWorkshops : Request.CreateWorkshops -> Async<unit>
    CreateOnlineLessons : Request.CreateOnlineLessons -> Async<unit>
    GetOnlineLessons : DateTimeOffset * DateTimeOffset -> Async<Queries.OnlineLesson list>
    ChangeLessonDescription : Request.ChangeLessonDescription -> Async<unit>
}
with
    static member RouteBuilder _ m = sprintf "/api/admin/%s" m    