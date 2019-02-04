module Yobo.Shared.Validation

open System

type ValidationErrorType = 
    | IsEmpty
    | MustBeLongerThan of int
    | MustBeBiggerThan of int
    | MustBeAfter of DateTimeOffset
    | ValuesNotEqual of string
    | IsNotValidEmail
    | TermsNotAgreed
    with
        member x.Explain() =
            match x with
            | IsEmpty -> "Prosím vyplňte hodnotu."
            | MustBeLongerThan l -> sprintf "Hodnota musí být delší než %i znaků." l
            | ValuesNotEqual _ -> sprintf "Hodnoty se neshodují"
            | IsNotValidEmail -> "Vyplňte správný formát pro emailovou adresu."
            | MustBeBiggerThan v -> sprintf "Hodnota musí být větší než %i." v
            | MustBeAfter d -> sprintf "Hodnota musí být po datu %A." d
            | TermsNotAgreed -> "Potvrďte souhlas s obchodními podmínkami"

type ValidationError = {
    Field : string
    ErrorType : ValidationErrorType
}

type ValidationResult = {
    IsValid : bool
    Errors: ValidationError list
}
with
    static member FromErrorList (rows:ValidationError list) = {
        IsValid = rows.Length = 0
        Errors = rows
    }
    static member Empty = ValidationResult.FromErrorList []
    static member FromResult result =
        match result with
        | Ok _ -> ValidationResult.Empty
        | Error errs -> ValidationResult.FromErrorList errs

let validateTermsAgreed getter args =
    let value = args |> getter
    if value = false then TermsNotAgreed |> Some else None

let validateNotEmpty getter args = 
    let value = args |> getter
    if String.IsNullOrWhiteSpace(value) then IsEmpty |> Some else None

let validateLongerThan l getter args =
    let value : string = args |> getter
    if value.Length <= l then MustBeLongerThan l |> Some else None

let validateBiggerThan v getter args =
    let value : int = args |> getter
    if value <= v then MustBeBiggerThan v |> Some else None

let validateIsAfter d getter args =
    let value : DateTimeOffset = args |> getter
    if value <= d then MustBeAfter d |> Some else None

let validateIsBeforeAnother beforeGetter afterGetter args =
    let beforeValue : DateTimeOffset = args |> beforeGetter
    let afterValue : DateTimeOffset = args |> afterGetter
    if afterValue <= beforeValue then MustBeAfter afterValue |> Some else None

let validateEquals txt fstGetter sndGetter args =
    let val1 = args |> fstGetter
    let val2 = args |> sndGetter
    if val1 <> val2 then ValuesNotEqual txt |> Some else None

let private isValidEmail (value:string) =
    let parts = value.Split([|'@'|])
    if parts.Length < 2 then false
    else
        let lastPart = parts.[parts.Length - 1]
        lastPart.Split([|'.'|], StringSplitOptions.RemoveEmptyEntries).Length > 1

let validateNotEmptyGuid getter args =
    let value : Guid = args |> getter
    if value = Guid.Empty then IsEmpty |> Some else None

let validateEmail getter args =
    let value : string = args |> getter
    if isValidEmail value then None
    else IsNotValidEmail |> Some

let validate (arg:'a) (fns: (string * ('a -> ValidationErrorType option)) list) =
    fns 
    |> List.map (fun (name,fn) -> name, arg |> fn)
    |> List.filter (snd >> Option.isSome)
    |> List.map (fun (n,r) -> n, Option.get r)
    |> (fun errors -> 
        if errors.Length = 0 then Ok arg
        else errors |> List.map (fun (f,e) -> { Field = f; ErrorType = e }) |> Error
    )