module Yobo.Client.Router

open Feliz.Router
open Domain

let parseUrl = function
    | [ Paths.Login ] -> Auth(Auth.Domain.Login Auth.Login.Domain.Model.init)
    | [ Paths.Calendar ] -> Calendar
    | _ -> Model.init.CurrentPage
    