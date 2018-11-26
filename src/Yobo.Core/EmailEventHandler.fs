module Yobo.Core.EmailEventHandler

open Yobo.Libraries.Emails

let getHandler (sender:EmailProvider) (settings:EmailSettings.Settings) =
    let handle cmd =
        match cmd with
        | CoreEvent.Users e -> e |> Users.EmailEventHandler.handle settings |> sender.Send
    handle