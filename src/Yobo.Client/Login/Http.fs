module Yobo.Client.Login.Http

open System
open Fable.PowerPack
open Yobo.Client.Http
open Yobo.Client.Login.Domain
open Yobo.Shared.Communication
open Thoth.Json
open Yobo.Shared.Login.Domain

let login (login:Login) =
    promise {
        return! login |> asPost |> fetchAs Routes.login (Decode.Auto.generateDecoder<User>())
    } |> promiseToCmd<User, Msg> LoginDone