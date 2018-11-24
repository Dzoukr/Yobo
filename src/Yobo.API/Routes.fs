module Yobo.API.Routes

open Giraffe

let webApp : HttpHandler =
    subRoute "/api" <| choose [
        route "/register" >=> CompositionRoot.HttpHandlers.Login.register
    ]