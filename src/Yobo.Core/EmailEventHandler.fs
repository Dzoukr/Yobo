module Yobo.Core.EmailEventHandler

open Yobo.Libraries.Emails
open Yobo.Core.CQRS

let getHandler (userQueries:Users.ReadQueries.UserQueries) (sender:EmailProvider) (settings:EmailSettings.Settings) =

    let send = function
        | Some x -> x |> sender.Send
        | None -> Ok ()

    let handle cmd =
        match cmd with
        | CoreEvent.Users e -> e |> Users.EmailEventHandler.handle userQueries settings |> send
        | CoreEvent.Lessons _
        | CoreEvent.Workshops _
        | CoreEvent.UsersRegistry _ -> Ok ()
    handle