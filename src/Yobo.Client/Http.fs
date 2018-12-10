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

let registrationAPI =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Registration.Communication.routeBuilder
    |> Remoting.buildProxy<Yobo.Shared.Registration.Communication.API>

let loginAPI =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Yobo.Shared.Login.Communication.routeBuilder
    |> Remoting.buildProxy<Yobo.Shared.Login.Communication.API>