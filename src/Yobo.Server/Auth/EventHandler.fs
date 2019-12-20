module Yobo.Server.Auth.EventHandler

open Domain
open Microsoft.Data.SqlClient
open FSharp.Control.Tasks

let private handleDb (e:Event) =
    task {
        return ()
    }
    
let handle (conn:SqlConnection) (evns:Event list) =
    task {
        for evn in evns do
            do! handleDb evn
    }
    