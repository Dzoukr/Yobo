module Yobo.API.Routes

open Giraffe
open Yobo.Shared.Communication

let frontend wwwRootPath =
    let wwwRootPath = if isNull wwwRootPath then "" else wwwRootPath
    Giraffe.ResponseWriters.htmlFile <| System.IO.Path.Combine(wwwRootPath, "index.html")

let webApp wwwRootPath : HttpHandler = choose [
    route Routes.register >=> CompositionRoot.HttpHandlers.Registration.register
    routeCif Routes.activateAccount CompositionRoot.HttpHandlers.Registration.activateAccount
    frontend wwwRootPath
]