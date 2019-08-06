module Yobo.Core.Auth.EmailEventHandler

open System
open Yobo.Libraries.Emails
open Yobo.Core
open Yobo.Shared.Communication

let handle getById (settings:EmailSettings.Settings) = function
    | Registered args ->
        let name = sprintf "%s %s" args.FirstName args.LastName
        let tos = { Email = args.Email; Name = name }
        let subject = "Registrace Mindful Yoga"
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
        |> getById
        |> Option.map (fun (user:Yobo.Shared.Domain.User) ->
            let name = sprintf "%s %s" user.FirstName user.LastName
            let tos = { Email = user.Email; Name = name }
            let subject = "Registrace Mindful Yoga"
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
    | PasswordResetInitiated args ->
        args.Id
        |> getById
        |> Option.map (fun user ->
            let name = sprintf "%s %s" user.FirstName user.LastName
            let tos = { Email = user.Email; Name = name }
            let subject = "Požadavek na změnu hesla"
            let message =
                Fue.Data.init
                |> Fue.Data.add "reset" (Uri(settings.BaseUrl, sprintf FrontendRoutes.resetPassword args.PasswordResetKey))
                |> Fue.Compiler.fromTextSafe (EmailTemplateLoader.loadTemplate "Users.EmailTemplates.ResetPassword.html")

            { EmailMessage.Empty with
                From = settings.From
                To = tos |> List.singleton
                Subject = subject
                HtmlMessage = message
            }
        )
    | Activated _
    | PasswordReset _ -> None