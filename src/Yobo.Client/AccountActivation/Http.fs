module Yobo.Client.AccountActivation.Http

open System
open Fable.PowerPack
open Yobo.Client.Http
open Yobo.Client.AccountActivation.Domain
open Yobo.Shared.Communication
open Thoth.Json

let activate (id:Guid) =
    promise {
        let url = sprintf Routes.activateAccount id
        return! id |> asPost |> fetchAs url Decode.guid
    } |> promiseToCmd<Guid, Msg> ActivateDone