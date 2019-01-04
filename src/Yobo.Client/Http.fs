module Yobo.Client.Http

open Fable.Remoting.Client

module Cmd =
    open Elmish
    open Yobo.Shared.Communication

    let ofAsyncResult f msg i =
        Cmd.ofAsync
            f i            
            (msg)
            (fun ex -> ServerError.Exception(ex.Message) |> Error |> msg)

let usersAPI =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Users.Communication.routeBuilder
    |> Remoting.buildProxy<Yobo.Shared.Users.Communication.API>