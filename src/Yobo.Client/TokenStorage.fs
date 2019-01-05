module Yobo.Client.TokenStorage

open System
open Elmish
open Fable.Import

let private storageKey = "token"

let tryGetToken () : string option =
    Browser.localStorage.getItem(storageKey)
    |> (function null -> None | x -> Some (unbox x))
    |> Option.bind (fun x -> if String.IsNullOrWhiteSpace(x) then None else Some x)

let removeToken () = Browser.localStorage.removeItem(storageKey)

let setToken (token:string) =
    if String.IsNullOrWhiteSpace(token) then removeToken()
    else Browser.localStorage.setItem(storageKey, token)