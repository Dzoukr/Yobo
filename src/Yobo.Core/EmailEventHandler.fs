module Yobo.Core.EmailEventHandler

open Yobo.Libraries.Emails

let getHandler (userQueries:Users.ReadQueries.UserQueries<_>) (sender:EmailProvider) (settings:EmailSettings.Settings) =

    let send = function
        | Some x -> x |> sender.Send
        | None -> Ok ()

    let handle cmd =
        match cmd with
        | CoreEvent.Users e -> e |> Users.EmailEventHandler.handle userQueries settings |> send
    handle