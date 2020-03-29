module Yobo.Server.Core.Admin.HttpHandlers

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

let private adminService (root:CompositionRoot) userId : AdminService =
    {
        GetAllUsers = root.Admin.Queries.GetAllUsers >> Async.AwaitTask
        AddCredits = ServerError.validate validateAddCredits >> (addCredits root.Admin) >> Async.AwaitTask
        SetExpiration = ServerError.validate validateSetExpiration >> (setExpiration root.Admin) >> Async.AwaitTask
    }

let adminServiceHandler (root:CompositionRoot) : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder AdminService.RouteBuilder
    |> Remoting.fromContext (Auth.HttpHandlers.withUser (adminService root))
    |> Remoting.withErrorHandler Remoting.errorHandler
    |> Remoting.buildHttpHandler