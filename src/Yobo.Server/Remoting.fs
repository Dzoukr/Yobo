module Yobo.Server.Remoting

open System
open Fable.Remoting.Server
open Microsoft.AspNetCore.Http
open Yobo.Shared.Domain

let rec errorHandler (ex: Exception) (routeInfo: RouteInfo<HttpContext>) = 
    match ex with
    | :? AggregateException as ag -> errorHandler ag.InnerException routeInfo
    | ServerException err -> Propagate err
    | _ -> Propagate (ServerError.Exception(ex.Message))