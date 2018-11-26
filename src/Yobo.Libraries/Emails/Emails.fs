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
with
    static member Empty = {
        From = { Name = ""; Email = ""}
        To = []
        Cc = []
        Bcc = []
        Subject = ""
        HtmlMessage = ""
        PlainTextMessage = ""
    }

type EmailProvider = {
    Send : EmailMessage -> Result<unit, Exception>
}