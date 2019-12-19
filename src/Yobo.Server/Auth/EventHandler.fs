module Yobo.Server.Auth.EventHandler

open Domain

let private handleDb (e:Event) =
    ()
    
let handle (evns:Event list) =
    evns |> List.iter handleDb
    