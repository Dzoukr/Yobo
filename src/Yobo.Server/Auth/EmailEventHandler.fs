module Yobo.Server.Auth.EmailEventHandler

open System.Threading.Tasks
open Domain
open FSharp.Control.Tasks
open Yobo.Libraries.Emails
open Yobo.Server.EmailTemplates
    
let handle (sendEmail:{| To:Address; Subject:string; Message:string |} -> Task<unit>) (templateBuilder:EmailTemplateBuilder) (e:Event) =
    task {
        match e with
        | Registered args ->
            let name = sprintf "%s %s" args.FirstName args.LastName
            let tos = { Email = args.Email; Name = name }
            let subject = "Registrace Mindful Yoga"
            let message = args.ActivationKey |> templateBuilder.RegisterEmailMessage
            do! {| To = tos; Subject = subject; Message = message |} |> sendEmail
            return ()
        | _ -> return ()
    }