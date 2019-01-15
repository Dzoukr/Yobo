module Yobo.API.Admin.Functions

open System
open Yobo.Shared.Admin.Domain
open FSharp.Rop
open Yobo.Core.Users
open Yobo.Core
open Yobo.Shared.Admin
open Yobo.Shared.Communication
open Yobo.API.Extensions

module ArgsBuilder =
    open Yobo.API

    let buildAddCredits =
        ArgsBuilder.build (fun (acc:AddCredits) ->
            ({
                Id = acc.UserId
                Credits = acc.Credits
                ExpirationUtc = acc.ExpirationUtc
            } : CmdArgs.AddCredits)
        ) Validation.validateAddCredits
        >> Result.mapError ServerError.ValidationError

    //let buildAddLessons =
    //    ArgsBuilder.build (fun (acc:AddLesson) ->
    //        ({
    //            Id = acc.UserId
    //            Credits = acc.Credits
    //            ExpirationUtc = acc.ExpirationUtc
    //        } : CmdArgs.AddLesson)
    //    ) Validation.validateAddLesson
    //    >> Result.mapError ServerError.ValidationError


let mapToUser (u:Yobo.Core.Users.ReadQueries.User) =
    {
        Id = u.Id
        Email = u.Email
        FirstName = u.FirstName
        LastName = u.LastName
        ActivatedUtc = u.ActivatedUtc |> Option.map (fun x -> x.ToUtc())
        Credits = u.Credits
        CreditsExpirationUtc = u.CreditsExpirationUtc
    } : Yobo.Shared.Admin.Domain.User

let addCredits cmdHandler (acc:AddCredits) =
    result {
        let! args = acc |> ArgsBuilder.buildAddCredits
        let! _ = args |> Command.AddCredits |> CoreCommand.Users |> cmdHandler
        return ()
    }

let addLessons cmdHandler (acc:AddLesson list) =
    result {
        //let! args = acc |> ArgsBuilder.buildAddCredits
        //let! _ = args |> Command.AddCredits |> CoreCommand.Users |> cmdHandler
        return ()
    }