namespace Yobo.Libraries.Emails

open System

type Address = {
    Email : string
    Name : string
}

type EmailMessage = {
    From: Address
    To : Address list
    Cc : Address list
    Bcc : Address list
    Subject : string
    HtmlMessage : string
    PlainTextMessage : string
}

type EmailProvider = {
    Send : EmailMessage -> Result<unit, Exception>
}