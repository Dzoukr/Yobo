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

let authAPI =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Auth.Communication.routeBuilder
    |> Remoting.buildProxy<Yobo.Shared.Auth.Communication.API>