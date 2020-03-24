module Yobo.Server.Remoting

open System
open Fable.Remoting.Server
open Microsoft.AspNetCore.Http
open Yobo.Shared.Domain

let private statusCode = function
    | Exception _ -> 500
    | _ -> 400

let rec errorHandler (ex: Exception) (routeInfo: RouteInfo<HttpContext>) = 
    match ex with
    | :? AggregateException as ag -> errorHandler ag.InnerException routeInfo
    | ServerException err ->
        routeInfo.httpContext.Response.StatusCode <- err |> statusCode
        Propagate err
    | _ -> Propagate (ServerError.Exception(ex.Message))