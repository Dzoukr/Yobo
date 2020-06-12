module Yobo.Libraries.Emails.MailjetSender

open Mailjet.Client
open Mailjet.Client.Resources
open Newtonsoft.Json.Linq

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
    client.PostAsync(request)

let sendEmail (apiKey:string) (secretKey:string) =
    let client = MailjetClient(apiKey, secretKey)
    client.Version <- ApiVersion.V3_1
    send client
    