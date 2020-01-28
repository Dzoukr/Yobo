module Yobo.Shared.Validation

open System

type ValidationErrorType =
    | IsEmpty
    | IsNotValidEmail
    | IsBelowMinimalLength of int
    | PasswordsDontMatch
    | TermsNotAgreed

module ValidationErrorType =
    let explain = function
        | IsEmpty -> "Prosím vyplňte hodnotu."
        | IsNotValidEmail -> "Vyplňte správný formát pro emailovou adresu."
        | IsBelowMinimalLength l -> sprintf "Hodnota musí být nejméně %i znaků." l
        | PasswordsDontMatch -> "Hesla se neshodují."
        | TermsNotAgreed -> "Potvrďte souhlas s obchodními podmínkami."

type ValidationError = {
    Field : string
    Type : ValidationErrorType
}

module ValidationError =
    let explain err =
        sprintf "Pole \"%s\" obsahuje chybu: \"%s\"" err.Field (err.Type |> ValidationErrorType.explain)

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
    
let validateMinimumLength l (value:string) =
    if value.Length < l then IsBelowMinimalLength l |> Some else None
    
let validatePasswordsMatch val1 val2 =
    if val1 <> val2 then PasswordsDontMatch |> Some else None

let validateTermsAgreed value =
    if value = false then TermsNotAgreed |> Some else None
    
let validate conds =
    conds
    |> List.filter (snd >> Option.isSome)
    |> List.map (fun (n,e) -> n, Option.get e)
    |> List.map (fun (f,t) -> { Field = f; Type = t })