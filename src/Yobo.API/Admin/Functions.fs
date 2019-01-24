module Yobo.API.Admin.Functions

open System
open Yobo.Shared.Admin.Domain
open FSharp.Rop
open Yobo.Core.Users
open Yobo.Core
open Yobo.Core.CQRS
open Yobo.Shared.Admin
open Yobo.Shared.Communication

module ArgsBuilder =
    open Yobo.API

    let buildAddCredits =
        ArgsBuilder.build (fun (x:AddCredits) ->
            ({
                Id = x.UserId
                Credits = x.Credits
                Expiration = x.Expiration
            } : Users.CmdArgs.AddCredits)
        ) Validation.validateAddCredits
        >> Result.mapError ServerError.ValidationError

    let buildAddLessons =
        ArgsBuilder.build (fun (x:AddLesson) ->
            ({
                Id = Guid.NewGuid()
                StartDate = x.Start
                EndDate = x.End
                Name = x.Name
                Description = x.Description
            } : Lessons.CmdArgs.Create)
        ) Validation.validateAddLesson
        >> Result.mapError ServerError.ValidationError


let addCredits cmdHandler (acc:AddCredits) =
    result {
        let! args = acc |> ArgsBuilder.buildAddCredits
        let! _ = args |> Users.Command.AddCredits |> CoreCommand.Users |> cmdHandler
        return ()
    }

let addLessons cmdHandler (acc:AddLesson list) =
    result {
        let! args = acc |> Result.traverse ArgsBuilder.buildAddLessons
        let! _ = args |> Result.traverse (Lessons.Command.Create >> CoreCommand.Lessons >> cmdHandler)
        return ()
    }

let cancelLesson cmdHandler (i:Guid) =
    result {
        let! _ = ({ Id = i } : Lessons.CmdArgs.Cancel) |> Lessons.Command.Cancel |> CoreCommand.Lessons |> cmdHandler
        return ()
    }