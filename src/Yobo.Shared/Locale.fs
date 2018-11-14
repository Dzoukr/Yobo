module Yobo.Shared.Locale

open System
open Yobo.Shared.Validation
open Yobo.Shared.Text

let toCz = function
    | FirstName -> "křestní jméno"
    | LastName -> "příjmení"
    | Password -> "heslo"
    | SecondPassword -> "heslo (pro kontrolu)"
    | Email -> "email"

let title (s:string) = Char.ToUpperInvariant(s.[0]).ToString() + s.Substring(1).ToLowerInvariant()

let errorToCz field err =
    let field = field |> toCz
    match err with
    | IsEmpty -> sprintf "Prosím vyplňte %s" field
    | MustBeLongerThan l -> sprintf "%s musí být delší než %i znaků" (title field) l
    | ValuesNotEqual -> sprintf "Hodnota u pole %s se neshoduje" field