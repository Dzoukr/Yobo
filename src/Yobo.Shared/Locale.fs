[<RequireQualifiedAccess>]
module Yobo.Shared.Locale

open System
open Yobo.Shared.Validation
open Yobo.Shared.Text
open Yobo.Shared.Domain
open Yobo.Shared.Communication

let toCz = function
    | Id -> "ID"
    | FirstName -> "křestní jméno"
    | LastName -> "příjmení"
    | Password -> "heslo"
    | SecondPassword -> "heslo (pro kontrolu)"
    | Email -> "email"
    | Registration -> "registrace"
    | Register -> "zaregistrovat se"
    | Login -> "přihlásit se"
    
let toCzMsg = function    
    | ActivatingYourAccount -> "Aktivuji váš účet..."
    | AccountSuccessfullyActivated -> "Váš účet byl právě zaktivován. Nyní se můžete přihlásit do systému."

let title (s:string) = Char.ToUpperInvariant(s.[0]).ToString() + s.Substring(1).ToLowerInvariant()

let toTitleCz = toCz >> title

let validationErrorToCz err =
    match err with
    | IsEmpty field -> sprintf "Prosím vyplňte %s." (toCz field)
    | MustBeLongerThan (field, l) -> sprintf "%s musí být delší než %i znaků." (field |> toTitleCz) l
    | ValuesNotEqual(f1,f2) -> sprintf "Hodnota u pole %s se neshoduje s hodnotu v poli %s." (toCz f1) (toCz f2)
    | IsNotValidEmail field -> sprintf "%s není ve správném formátu pro emailovou adresu." (toTitleCz field)

let domainErrorToCz err =
    match err with
    | DomainError.ActivationKeyDoesNotMatch -> "Aktivační klíč nebyl nalezen."
    | DomainError.UserAlreadyActivated -> "Uživatelský účet je již zaktivován."
    | DomainError.ItemAlreadyExists field -> sprintf "Tento %s již v systému existuje." (field |> toCz)
    | DomainError.ItemDoesNotExist field -> sprintf "Položka s touto hodnotou %s v systému neexistuje." (field |> toCz)

let serverErrorToCz (err:Yobo.Shared.Communication.ServerError) =
    match err with
    | Exception ex -> sprintf "Došlo k chybě : %s" ex
    | DomainError err -> err |> domainErrorToCz |> sprintf "Došlo k chybě : %s" 
    | ValidationError _ -> "Došlo k chybě správnosti dat. Prosím zkontrolujte formulář."