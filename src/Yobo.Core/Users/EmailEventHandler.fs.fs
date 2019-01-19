module Yobo.Core.Users.EmailEventHandler

open System
open Yobo.Libraries.Emails
open Yobo.Core
open Yobo.Shared.Communication
open FSharp.Rop

let handle (q:ReadQueries.UserQueries<_>) (settings:EmailSettings.Settings) = function
    | Registered args ->
        let name = sprintf "%s %s" args.FirstName args.LastName
        let tos = { Email = args.Email; Name = name }
        let subject = "TODO"
        let message =
            Fue.Data.init
            |> Fue.Data.add "activate" (Uri(settings.BaseUrl, sprintf FrontendRoutes.activateAccount args.ActivationKey))
            |> Fue.Compiler.fromTextSafe (EmailTemplateLoader.loadTemplate "Users.EmailTemplates.Register.html")

        { EmailMessage.Empty with
            From = settings.From
            To = tos |> List.singleton
            Subject = subject
            HtmlMessage = message
        } |> Some

    | ActivationKeyRegenerated args ->
        args.Id
        |> q.GetById
        |> Result.toOption
        |> Option.map (fun user ->
            let name = sprintf "%s %s" user.FirstName user.LastName
            let tos = { Email = user.Email; Name = name }
            let subject = "TODO"
            let message =
                Fue.Data.init
                |> Fue.Data.add "activate" (Uri(settings.BaseUrl, sprintf FrontendRoutes.activateAccount args.ActivationKey))
                |> Fue.Compiler.fromTextSafe (EmailTemplateLoader.loadTemplate "Users.EmailTemplates.RegenerateActivation.html")

            { EmailMessage.Empty with
                From = settings.From
                To = tos |> List.singleton
                Subject = subject
                HtmlMessage = message
            }
        )

    | Activated _
    | CashReservationsBlocked _
    | CashReservationsUnblocked _
    | CreditsRefunded _
    | CreditsWithdrawn _
    | CreditsAdded _ ->
        None