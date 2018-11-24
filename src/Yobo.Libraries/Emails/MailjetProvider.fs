module Yobo.Libraries.Emails.MailjetProvider

open Mailjet.Client
open Mailjet.Client.Resources
open Newtonsoft.Json.Linq
open FSharp.Control.Tasks.V2
open System

type Configuration = {
    ApiKey : string
    SecretKey : string
}

let private toAddress (add:Address) = 
    let o = JObject()
    o.["Email"] <- JValue(add.Email)
    o.["Name"] <- JValue(add.Name)
    o

let private toMessage (msg:EmailMessage) =
    let o = JObject()
    o.["From"] <- msg.From |> toAddress
    o.["To"] <- msg.To |> List.map toAddress |> JArray
    o.["Cc"] <- msg.Cc |> List.map toAddress |> JArray
    o.["Bcc"] <- msg.Bcc |> List.map toAddress |> JArray
    o.["Subject"] <- JValue msg.Subject
    o.["TextPart"] <- JValue msg.PlainTextMessage
    o.["HTMLPart"] <- JValue msg.HtmlMessage
    o

let private send (client:MailjetClient) (msg:EmailMessage) =
    let request = MailjetRequest()
    request.Resource <- Send.Resource
    request.Property(Send.Messages, msg |> toMessage |> JArray) |> ignore

    task {
        let! resp = client.PostAsync(request)
        if resp.IsSuccessStatusCode then return Ok ()
        else return (Exception(resp.GetErrorInfo() + resp.GetErrorMessage())) |> Error
    }

let create (conf:Configuration) =
    let client = MailjetClient(conf.ApiKey, conf.SecretKey)
    client.Version <- ApiVersion.V3_1
    {
        Send = send client >> Async.AwaitTask >> Async.RunSynchronously
    }
