module Yobo.Client.Router

open Feliz.Router

type Page =
    | Login

let defaultPage = Login
    
let parseUrl = function
    | [ "login" ] -> Login 
    | _ -> defaultPage
    
let getHref = function
    | Login -> Router.format("login")
    