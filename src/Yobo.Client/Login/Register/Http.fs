module Yobo.Client.Login.Register.Http

open Elmish
open Fable.PowerPack
open Yobo.Client.Login.Register.Domain
open Yobo.Shared.Login.Register.Domain
open Yobo.Shared.Communication
open Thoth.Json
open Fable.PowerPack.Fetch
open Fable.Import

let private toJson v = Encode.Auto.toString(0, v)
//let private errorDecoder = Decode.Auto.generateDecoder<ServerError>()
let tryToServerError s =
    match Decode.Auto.fromString<ServerError>(s) with
    | Ok err -> Some err
    | Error _ -> None

let fetch url (init: RequestProperties list) =
    promise { 
        let! resp = GlobalFetch.fetch(RequestInfo.Url url, requestProps init)
        match resp.Ok with
        | true -> return (Ok resp)
        | false ->
            let! respText = resp.text()
            match respText |> tryToServerError with
            | Some err -> return Error err
            | None -> return ServerError.Exception(new System.Exception(respText)) |> Error
    }

let fetchAs<'a> url (decoder:Decode.Decoder<'a>) init =
    let safeDecode = Decode.fromString decoder >> Result.mapError (System.Exception >> ServerError.Exception)
    promise {
        let! res = fetch url init
        match res with
        | Ok r -> 
            let! txt = r.text()
            return txt |> safeDecode
        | Error r -> return Error r
    }

let private asPost r =
    [ RequestProperties.Method HttpMethod.POST
      requestHeaders [ContentType "application/json"]
      RequestProperties.Body <| unbox(toJson r)]

let registerPromise (acc:Account) =
    promise {
        return! acc |> asPost |> fetchAs "/api/register" Decode.guid
    }

let register (acc:Account) =
    Cmd.ofPromise registerPromise acc
                  (fun s -> RegisterDone s)
                  (fun ex -> 
                    Browser.console.log "HERE"
                    Exception(ex) |> Error |> RegisterDone)