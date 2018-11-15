[<RequireQualifiedAccess>]
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
    | Registration -> "registrace"
    | ToRegister -> "zaregistrovat se"

let title (s:string) = Char.ToUpperInvariant(s.[0]).ToString() + s.Substring(1).ToLowerInvariant()

let toTitleCz = toCz >> title

let errorToCz err =
    match err with
    | IsEmpty field -> sprintf "Prosím vyplňte %s" (toCz field)
    | MustBeLongerThan (field, l) -> sprintf "%s musí být delší než %i znaků" (field |> toTitleCz) l
    | ValuesNotEqual(f1,f2) -> sprintf "Hodnota u pole %s se neshoduje s hodnotu v poli %s" (toCz f1) (toCz f2)