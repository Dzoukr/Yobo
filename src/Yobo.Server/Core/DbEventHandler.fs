module Yobo.Server.Core.DbEventHandler

open Microsoft.Data.SqlClient
open FSharp.Control.Tasks
open Yobo.Server.Core.Domain

let handle (conn:SqlConnection) (e:Event) =
    task {
        match e with
        | CreditsAdded args -> do! args |> Database.Updates.creditsAdded conn
        
    }