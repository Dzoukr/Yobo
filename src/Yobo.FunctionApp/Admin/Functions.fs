module Yobo.FunctionApp.Admin.Functions

open System
open Yobo.Shared.Admin.Domain
open FSharp.Rop
open Yobo.Core.Auth
open Yobo.Core
open Yobo.Shared.Admin
open Yobo.Shared.Communication
open Yobo.Shared.Domain

module ArgsBuilder =
    open Yobo.FunctionApp

    let buildAddCredits =
        ArgsBuilder.build (fun (x:AddCredits) ->
            ({
                UserId = x.UserId
                Credits = x.Credits
                Expiration = x.Expiration
            } : Lessons.CmdArgs.AddCredits)
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
            } : Lessons.CmdArgs.CreateLesson)
        ) Validation.validateAddLesson
        >> Result.mapError ServerError.ValidationError

    let buildAddWorkshop =
        ArgsBuilder.build (fun (x:AddWorkshop) ->
            ({
                Id = Guid.NewGuid()
                StartDate = x.Start
                EndDate = x.End
                Name = x.Name
                Description = x.Description
            } : Lessons.CmdArgs.CreateWorkshop)
        ) Validation.validateAddWorkshop
        >> Result.mapError ServerError.ValidationError


let addCredits getProjection cmdHandler (acc:AddCredits) =
    result {
        let! proj = getProjection acc.UserId |> Result.ofOption (DomainError.ItemDoesNotExist "Id" |> ServerError.DomainError)
        let! args = acc |> ArgsBuilder.buildAddCredits
        let! _ = args |> cmdHandler proj
        return ()
    }

let addLessons getProjection cmdHandler (acc:AddLesson list) =
    result {
        let proj = getProjection ()
        let! args = acc |> Result.traverse ArgsBuilder.buildAddLessons
        let! _ = args |> Result.traverse (cmdHandler proj)
        return ()
    }

let addWorkshops getProjection cmdHandler (acc:AddWorkshop list) =
    result {
        let proj = getProjection ()
        let! args = acc |> Result.traverse ArgsBuilder.buildAddWorkshop
        let! _ = args |> Result.traverse (cmdHandler proj)
        return ()
    }

let cancelLesson getProjection cmdHandler (i:Guid) =
    result {
        let! proj = getProjection i |> Result.ofOption (DomainError.ItemDoesNotExist "Id" |> ServerError.DomainError)
        let! _ = ({ Id = i } : Lessons.CmdArgs.CancelLesson) |> cmdHandler proj
        return ()
    }

let deleteLesson getProjection cmdHandler (i:Guid) =
    result {
        let! proj = getProjection i |> Result.ofOption (DomainError.ItemDoesNotExist "Id" |> ServerError.DomainError)
        let! _ = ({ Id = i } : Lessons.CmdArgs.DeleteLesson) |> cmdHandler proj
        return ()
    }

let deleteWorkshop getProjection cmdHandler (i:Guid) =
    result {
        let! proj = getProjection i |> Result.ofOption (DomainError.ItemDoesNotExist "Id" |> ServerError.DomainError)
        let! _ = ({ Id = i } : Lessons.CmdArgs.DeleteWorkshop) |> cmdHandler proj
        return ()
    }