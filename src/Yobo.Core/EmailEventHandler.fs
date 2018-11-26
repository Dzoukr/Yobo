module Yobo.Core.EmailEventHandler

open Yobo.Libraries.Emails

let getHandler (sender:EmailProvider) (from:Address) =
    let send = fun (msg:EmailMessage) ->  { msg with From = from } |> sender.Send
    let handle cmd =
        match cmd with
        | CoreEvent.Users e -> e |> Users.EmailEventHandler.handle |> send
    handle