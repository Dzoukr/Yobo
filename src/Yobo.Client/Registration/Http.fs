module Yobo.Client.Registration.Http

open Elmish
open Fable.PowerPack
open Yobo.Client.Registration.Domain
open Yobo.Shared.Registration.Domain
open Yobo.Shared.Communication
open Thoth.Json

open Yobo.Client.Http

let registrationPromise (acc:Account) =
    promise {
        return! acc |> asPost |> fetchAs Routes.register Decode.guid
    }

let registration (acc:Account) =
    Cmd.ofPromise registrationPromise acc
                  (fun s -> RegisterDone s)
                  (fun ex -> Exception(ex.Message) |> Error |> RegisterDone)