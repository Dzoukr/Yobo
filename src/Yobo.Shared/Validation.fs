module Yobo.Shared.Validation

open System
open Yobo.Shared.Text

type ValidationError = 
    | IsEmpty of TextValue
    | MustBeLongerThan of TextValue * int
    | MustBeBiggerThan of TextValue * int
    | MustBeAfter of TextValue * DateTime
    | ValuesNotEqual of TextValue * TextValue
    | IsNotValidEmail of TextValue


type ValidationResult = {
    IsValid : bool
    Errors: (ValidationError) list
}
with
    static member FromErrorList (errs:ValidationError list) = {
        IsValid = errs.Length = 0
        Errors = errs
    }
    static member Empty = ValidationResult.FromErrorList []
    static member FromResult result =
        match result with
        | Ok _ -> ValidationResult.Empty
        | Error errs -> ValidationResult.FromErrorList errs

let validateNotEmpty txt getter args = 
    let value = args |> getter
    if String.IsNullOrWhiteSpace(value) then IsEmpty(txt) |> Some else None

let validateLongerThan l txt getter args =
    let value : string = args |> getter
    if value.Length <= l then MustBeLongerThan(txt,l) |> Some else None

let validateBiggerThan v txt getter args =
    let value : int = args |> getter
    if value <= v then MustBeBiggerThan(txt,v) |> Some else None

let validateIsAfter d txt getter args =
    let value : DateTime = args |> getter
    if value <= d then MustBeAfter(txt,d) |> Some else None

let validateEquals fstTxt sndTxt fstGetter sndGetter args =
    let val1 = args |> fstGetter
    let val2 = args |> sndGetter
    if val1 <> val2 then ValuesNotEqual(fstTxt,sndTxt) |> Some else None

let private isValidEmail (value:string) =
    let parts = value.Split([|'@'|])
    if parts.Length < 2 then false
    else
        let lastPart = parts.[parts.Length - 1]
        lastPart.Split([|'.'|], StringSplitOptions.RemoveEmptyEntries).Length > 1

let validateNotEmptyGuid txt getter args =
    let value : Guid = args |> getter
    if value = Guid.Empty then IsEmpty(txt) |> Some else None

let validateEmail txt getter args =
    let value : string = args |> getter
    if isValidEmail value then None
    else IsNotValidEmail(txt) |> Some

let validate (arg:'a) (fns: ('a -> ValidationError option) list) =
    fns 
    |> List.map (fun (fn) -> arg |> fn)
    |> List.filter (Option.isSome)
    |> List.map (Option.get)
    |> (fun errors -> 
        if errors.Length = 0 then Ok arg
        else Error errors
    )