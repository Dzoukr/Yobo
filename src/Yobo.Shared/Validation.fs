module Yobo.Shared.Validation

open System

type ValidationErrorType =
    | IsEmpty
    | IsNotValidEmail

module ValidationErrorType =
    let explain = function
        | IsEmpty -> "Prosím vyplňte hodnotu."
        | IsNotValidEmail -> "Vyplňte správný formát pro emailovou adresu."

type ValidationError = {
    Field : string
    Type : ValidationErrorType
}

let validateNotEmpty value = 
    if String.IsNullOrWhiteSpace(value) then IsEmpty |> Some else None
    
let private isValidEmail (value:string) =
    let parts = value.Split([|'@'|])
    if parts.Length < 2 then false
    else
        let lastPart = parts.[parts.Length - 1]
        lastPart.Split([|'.'|], StringSplitOptions.RemoveEmptyEntries).Length > 1

let validateEmail value =
    if isValidEmail value then None
    else IsNotValidEmail |> Some
    
let validate conds =
    conds
    |> List.filter (snd >> Option.isSome)
    |> List.map (fun (n,e) -> n, Option.get e)
    |> List.map (fun (f,t) -> { Field = f; Type = t })