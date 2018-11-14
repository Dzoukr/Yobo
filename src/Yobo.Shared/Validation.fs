module Yobo.Shared.Validation

open System
open Yobo.Shared.Text

type ValidationError = 
    | IsEmpty of TextValue
    | MustBeLongerThan of TextValue * int
    | ValuesNotEqual of TextValue * TextValue

let private tryFindErrorForField errs txt =
    let findFn = function
        | IsEmpty t -> txt = t
        | MustBeLongerThan (t,_) -> txt = t
        | ValuesNotEqual (_, t2) -> txt = t2
    errs |> List.tryFind findFn

type ValidationResult = {
    IsValid : bool
    Errors: (ValidationError) list
    TryGetFieldError: TextValue -> ValidationError option
}
with
    static member FromErrorList (errs:ValidationError list) = {
        IsValid = errs.Length = 0
        Errors = errs
        TryGetFieldError = tryFindErrorForField errs
    }

    static member Empty = ValidationResult.FromErrorList []

let validateNotEmpty txt getter args = 
    let value = args |> getter
    if String.IsNullOrWhiteSpace(value) then IsEmpty(txt) |> Some else None

let validateLongerThan l txt getter args =
    let value : string = args |> getter
    if value.Length <= l then MustBeLongerThan(txt,l) |> Some else None 

let validateEquals fstTxt sndTxt fstGetter sndGetter args =
    let val1 = args |> fstGetter
    let val2 = args |> sndGetter
    if val1 <> val2 then ValuesNotEqual(fstTxt,sndTxt) |> Some else None

let validate (arg:'a) (fns: ('a -> ValidationError option) list) =
    fns 
    |> List.map (fun (fn) -> arg |> fn)
    |> List.filter (Option.isSome)
    |> List.map (Option.get)
    |> ValidationResult.FromErrorList