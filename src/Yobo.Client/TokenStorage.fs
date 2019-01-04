module Yobo.Client.TokenStorage

open Elmish
open Fable.Import

let private storageKey = "token"
let tryGetToken () : string option =
    Browser.localStorage.getItem(storageKey)
    |> function null -> None | x -> Some (unbox x)
let setToken token =
    Browser.localStorage.setItem(storageKey, token)