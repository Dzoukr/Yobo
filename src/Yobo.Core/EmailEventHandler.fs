module Yobo.Core.EmailEventHandler

open Yobo.Libraries.Emails

let getHandler (sender:EmailProvider) (settings:EmailSettings.Settings) =

    let send = function
        | Some x -> x |> sender.Send
        | None -> Ok ()

    let handle cmd =
        match cmd with
        | CoreEvent.Users e -> e |> Users.EmailEventHandler.handle settings |> send
    handle