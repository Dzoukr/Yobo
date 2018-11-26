module Yobo.Core.Users.EmailEventHandler

open Yobo.Libraries.Emails
open Yobo.Core
open Yobo.Shared.Communication


let handle (settings:EmailSettings.Settings) = function
    | Registered args ->
        let name = sprintf "%s %s" args.FirstName args.LastName
        let tos = { Email = args.Email; Name = name }
        let subject = "TODO"
        let message =
            Fue.Data.init
            |> Fue.Data.add "baseUrl" (settings.BaseUrl.ToString())
            |> Fue.Data.add "activate" (sprintf Routes.AccountActivation args.ActivationKey)
            |> Fue.Compiler.fromTextSafe (EmailTemplateLoader.loadTemplate "Users.EmailTemplates.Register.html")

        { EmailMessage.Empty with
            From = settings.From
            To = tos |> List.singleton
            Subject = subject
            HtmlMessage = message
        }