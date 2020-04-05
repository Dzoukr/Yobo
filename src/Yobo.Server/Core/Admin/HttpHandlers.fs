module Yobo.Server.Core.Admin.HttpHandlers

open System
open Giraffe
open Yobo.Server
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open FSharp.Control.Tasks
open Yobo.Shared.Errors
open Yobo.Shared.Core.Admin.Communication
open Yobo.Shared.Core.Admin.Validation
open Yobo.Shared.Core.Admin.Domain
open Yobo.Server.Core.Domain
open Yobo.Libraries.DateTime

let private addCredits (root:AdminRoot) (r:Request.AddCredits) =
    task {
        let args : CmdArgs.AddCredits =
            {
                UserId = r.UserId
                Credits = r.Credits
                Expiration = r.Expiration
            }
        return! root.CommandHandler.AddCredits args            
    }
    
let private setExpiration (root:AdminRoot) (r:Request.SetExpiration) =
    task {
        let args : CmdArgs.SetExpiration =
            {
                UserId = r.UserId
                Expiration = r.Expiration
            }
        return! root.CommandHandler.SetExpiration args            
    }
    
let private createLessons (root:AdminRoot) (r:Request.CreateLessons) =
    task {
        for d in r.Dates do
            let args : CmdArgs.CreateLesson =
                {
                    Id = Guid.NewGuid()
                    StartDate = d |> DateTimeOffset.toCzDateTimeOffset |> DateTimeOffset.withHoursMins r.StartTime
                    EndDate = d |> DateTimeOffset.toCzDateTimeOffset |> DateTimeOffset.withHoursMins r.EndTime
                    Name = r.Name
                    Description = r.Description
                    Capacity = r.Capacity
                }
            do! root.CommandHandler.CreateLesson args                
        return ()            
    }
    
let private createWorkshops (root:AdminRoot) (r:Request.CreateWorkshops) =
    task {
        for d in r.Dates do
            let args : CmdArgs.CreateWorkshop =
                {
                    Id = Guid.NewGuid()
                    StartDate = d |> DateTimeOffset.toCzDateTimeOffset |> DateTimeOffset.withHoursMins r.StartTime
                    EndDate = d |> DateTimeOffset.toCzDateTimeOffset |> DateTimeOffset.withHoursMins r.EndTime
                    Name = r.Name
                    Description = r.Description
                }
            do! root.CommandHandler.CreateWorkshop args                
        return ()            
    }    

let private createOnlineLessons (root:AdminRoot) (r:Request.CreateOnlineLessons) =
    task {
        for d in r.Dates do
            let args : CmdArgs.CreateOnlineLesson =
                {
                    Id = Guid.NewGuid()
                    StartDate = d |> DateTimeOffset.toCzDateTimeOffset |> DateTimeOffset.withHoursMins r.StartTime
                    EndDate = d |> DateTimeOffset.toCzDateTimeOffset |> DateTimeOffset.withHoursMins r.EndTime
                    Name = r.Name
                    Description = r.Description
                    Capacity = r.Capacity
                }
            do! root.CommandHandler.CreateOnlineLesson args                
        return ()            
    }
    
let private changeLessonDescription (root:AdminRoot) (r:Request.ChangeLessonDescription) =
    task {
        let args : CmdArgs.ChangeLessonDescription =
            {
                Id = r.Id
                Name = r.Name
                Description = r.Description
            }
        return! root.CommandHandler.ChangeLessonDescription args              
    }    

let private cancelLesson (root:AdminRoot) (r:Request.CancelLesson) =
    task {
        let args : CmdArgs.CancelLesson =
            {
                Id = r.Id
            }
        return! root.CommandHandler.CancelLesson args              
    }

let private deleteLesson (root:AdminRoot) (r:Request.DeleteLesson) =
    task {
        let args : CmdArgs.DeleteLesson =
            {
                Id = r.Id
            }
        return! root.CommandHandler.DeleteLesson args              
    }
    
let private deleteWorkshop (root:AdminRoot) (r:Request.DeleteWorkshop) =
    task {
        let args : CmdArgs.DeleteWorkshop =
            {
                Id = r.Id
            }
        return! root.CommandHandler.DeleteWorkshop args              
    }    

let private changeOnlineLessonDescription (root:AdminRoot) (r:Request.ChangeOnlineLessonDescription) =
    task {
        let args : CmdArgs.ChangeOnlineLessonDescription =
            {
                Id = r.Id
                Name = r.Name
                Description = r.Description
            }
        return! root.CommandHandler.ChangeOnlineLessonDescription args              
    }  

let private cancelOnlineLesson (root:AdminRoot) (r:Request.CancelOnlineLesson) =
    task {
        let args : CmdArgs.CancelOnlineLesson =
            {
                Id = r.Id
            }
        return! root.CommandHandler.CancelOnlineLesson args              
    }

let private deleteOnlineLesson (root:AdminRoot) (r:Request.DeleteOnlineLesson) =
    task {
        let args : CmdArgs.DeleteOnlineLesson =
            {
                Id = r.Id
            }
        return! root.CommandHandler.DeleteOnlineLesson args              
    }

let private adminService (root:CompositionRoot) userId : AdminService =
    {
        GetOnlineLessons = root.Admin.Queries.GetOnlineLessons >> Async.AwaitTask
        GetLessons = root.Admin.Queries.GetLessons >> Async.AwaitTask
        GetWorkshops = root.Admin.Queries.GetWorkshops >> Async.AwaitTask
        GetAllUsers = root.Admin.Queries.GetAllUsers >> Async.AwaitTask
        AddCredits = ServerError.validate validateAddCredits >> addCredits root.Admin >> Async.AwaitTask
        SetExpiration = ServerError.validate validateSetExpiration >> setExpiration root.Admin >> Async.AwaitTask
        CreateLessons = ServerError.validate validateCreateLessons >> createLessons root.Admin >> Async.AwaitTask
        CreateWorkshops = ServerError.validate validateCreateWorkshops >> createWorkshops root.Admin >> Async.AwaitTask
        CreateOnlineLessons = ServerError.validate validateCreateOnlineLessons >> createOnlineLessons root.Admin >> Async.AwaitTask
        ChangeLessonDescription = ServerError.validate validateChangeLessonDescription >> changeLessonDescription root.Admin >> Async.AwaitTask
        CancelLesson = ServerError.validate validateCancelLesson >> cancelLesson root.Admin >> Async.AwaitTask
        DeleteLesson = ServerError.validate validateDeleteLesson >> deleteLesson root.Admin >> Async.AwaitTask
        DeleteWorkshop = ServerError.validate validateDeleteWorkshop >> deleteWorkshop root.Admin >> Async.AwaitTask
        ChangeOnlineLessonDescription = ServerError.validate validateChangeOnlineLessonDescription >> changeOnlineLessonDescription root.Admin >> Async.AwaitTask
        CancelOnlineLesson = ServerError.validate validateCancelOnlineLesson >> cancelOnlineLesson root.Admin >> Async.AwaitTask
        DeleteOnlineLesson = ServerError.validate validateDeleteOnlineLesson >> deleteOnlineLesson root.Admin >> Async.AwaitTask
    }

let adminServiceHandler (root:CompositionRoot) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder AdminService.RouteBuilder
    |> Remoting.fromContext (Auth.HttpHandlers.withUser (adminService root))
    |> Remoting.withErrorHandler Remoting.errorHandler
    |> Remoting.buildHttpHandler