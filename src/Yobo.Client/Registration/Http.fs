module Yobo.Client.Registration.Http

open Fable.PowerPack
open Yobo.Client.Http
open Yobo.Client.Registration.Domain
open Yobo.Shared.Registration.Domain
open Yobo.Shared.Communication
open Thoth.Json

let register (acc:Account) =
    promise {
        return! acc |> asPost |> fetchAs Routes.register Decode.guid
    } |> promiseToCmd RegisterDone