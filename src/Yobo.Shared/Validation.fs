module Yobo.Shared.Validation

open System
open Yobo.Shared.Text

type ValidationError = 
    | IsEmpty
    | MustBeLongerThan of int
    | ValuesNotEqual

type ValidationResult = {
    IsValid : bool
    Errors: (TextValue * ValidationError) list
    TryGetError: TextValue -> ValidationError option
}
with
    static member FromErrorList (errs:(TextValue * ValidationError) list) = {
        IsValid = errs.Length = 0
        Errors = errs
        TryGetError = (fun x -> errs |> List.tryFind (fun (e,_) -> e = x ) |> Option.map snd)
    }

    static member Empty = ValidationResult.FromErrorList []

let validateNotEmpty getter args = 
    let value = args |> getter
    if String.IsNullOrWhiteSpace(value) then IsEmpty |> Some else None

let validateLongerThan l getter args =
    let value : string = args |> getter
    if value.Length <= l then MustBeLongerThan(l) |> Some else None 

let validateEquals fstGetter sndGetter args =
    let val1 = args |> fstGetter
    let val2 = args |> sndGetter
    if val1 <> val2 then ValuesNotEqual |> Some else None


let validate (arg:'a) (fns: (TextValue * ('a -> ValidationError option)) list) =
    fns 
    |> List.map (fun (n,fn) -> n, (arg |> fn))
    |> List.filter (snd >> Option.isSome)
    |> List.map (fun (n,e) -> n, Option.get(e))
    |> ValidationResult.FromErrorList