module Yobo.Client.Http

open Fable.PowerPack
open Yobo.Shared.Communication
open Thoth.Json
open Fable.PowerPack.Fetch
open Elmish
open Fable.Import

let private toJson v = Encode.Auto.toString(0, v)

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
            | None -> return ServerError.Exception(respText) |> Error
    }

let fetchAs<'a> url (decoder:Decode.Decoder<'a>) init =
    let safeDecode = Decode.fromString decoder >> Result.mapError ServerError.Exception
    promise {
        let! res = fetch url init
        match res with
        | Ok r -> 
            let! txt = r.text()
            return txt |> safeDecode
        | Error r -> return Error r
    }

let asPost r =
    [ RequestProperties.Method HttpMethod.POST
      requestHeaders [ContentType "application/json"]
      RequestProperties.Body <| unbox(toJson r)]

let promiseToCmd<'a,'b> msg (p:Fable.Import.JS.Promise<Result<'a, ServerError>>) : Cmd<'b> =
    Cmd.ofPromise (fun _ -> p) ()
        (fun s -> msg s)
        (fun ex -> Exception(ex.Message) |> Error |> msg)