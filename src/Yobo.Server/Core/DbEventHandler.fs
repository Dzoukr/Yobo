module Yobo.Server.Core.DbEventHandler

open Microsoft.Data.SqlClient
open FSharp.Control.Tasks
open Yobo.Server.Core.Database.Tables
open Yobo.Server.Core.Domain

let handle (conn:SqlConnection) (e:Event) =
    task {
        match e with
        | CreditsAdded args -> do! args |> Database.Updates.creditsAdded conn
        | ExpirationSet args -> do! args |> Database.Updates.expirationSet conn
        | LessonCreated args -> do! args |> Database.Updates.lessonCreated conn
        | WorkshopCreated args -> do! args |> Database.Updates.workshopCreated conn 
        | OnlineLessonCreated args -> do! args |> Database.Updates.onlineLessonCreated conn
        | LessonDescriptionChanged args -> do! args |> Database.Updates.lessonDescriptionChanged conn
        | LessonCancelled args -> do! args |> Database.Updates.lessonCancelled conn
        | CashReservationsUnblocked args -> do! args |> Database.Updates.cashReservationsUnblocked conn
        | CreditRefunded args -> do! args |> Database.Updates.creditRefunded conn
        | LessonReservationCancelled args -> do! args |> Database.Updates.lessonReservationCancelled conn
        | LessonDeleted args -> do! args |> Database.Updates.lessonDeleted conn
        | WorkshopDeleted args -> do! args |> Database.Updates.workshopDeleted conn
    }