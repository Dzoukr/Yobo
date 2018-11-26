module Yobo.Core.Users.EmailEventHandler

open Yobo.Libraries.Emails
open Yobo.Core


let handle = function
    | Registered args ->
        let name = sprintf "%s %s" args.FirstName args.LastName
        let tos = { Email = args.Email; Name = name }
        let subject = "TODO"
        let message = EmailTemplateLoader.loadTemplate "Users.EmailTemplates.Register.html"

        { EmailMessage.Empty with
            To = tos |> List.singleton
            Subject = subject
            HtmlMessage = message
        }

            