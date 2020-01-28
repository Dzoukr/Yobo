module Yobo.Server.Auth.DbEventHandler

open Domain
open Microsoft.Data.SqlClient
open FSharp.Control.Tasks
open Yobo.Server.Auth
    
let handle (conn:SqlConnection) (e:Event) =
    task {
        match e with
        | Registered args -> do! args |> Database.Updates.registered conn
    }