module Yobo.Server.Auth.EmailEventHandler

open System
open System.Threading.Tasks
open Domain
open FSharp.Control.Tasks
open Yobo.Libraries.Emails
open Yobo.Server.EmailTemplates
    
let handle
    (sendEmail:{| To:Address; Subject:string; Message:string |} -> Task<unit>)
    (templateBuilder:EmailTemplateBuilder)
    (tryGetUserById:Guid -> Task<Domain.Queries.BasicUserView option>)
    (e:Event) =
    task {
        match e with
        | Registered args ->
            let name = sprintf "%s %s" args.FirstName args.LastName
            let tos = { Email = args.Email; Name = name }
            let subject = "Registrace Mindful Yoga"
            let message = args.ActivationKey |> templateBuilder.RegisterEmailMessage
            do! {| To = tos; Subject = subject; Message = message |} |> sendEmail
            return ()
        | PasswordResetInitiated args ->
            match! args.Id |> tryGetUserById with
            | Some user ->
                let name = sprintf "%s %s" user.FirstName user.LastName
                let tos = { Email = user.Email; Name = name }
                let subject = "Požadavek na změnu hesla"
                let message = args.PasswordResetKey |> templateBuilder.PasswordResetEmailMessage
                do! {| To = tos; Subject = subject; Message = message |} |> sendEmail
                return ()
            | None -> return ()
        | ActivationKeyRegenerated args ->
            match! args.Id |> tryGetUserById with
            | Some user ->
                let name = sprintf "%s %s" user.FirstName user.LastName
                let tos = { Email = user.Email; Name = name }
                let subject = "Registrace Mindful Yoga"
                let message = args.ActivationKey |> templateBuilder.RegisterEmailMessage
                do! {| To = tos; Subject = subject; Message = message |} |> sendEmail
                return ()
            | None -> return ()
        | _ -> return ()
    }