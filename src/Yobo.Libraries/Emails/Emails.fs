namespace Yobo.Libraries.Emails

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