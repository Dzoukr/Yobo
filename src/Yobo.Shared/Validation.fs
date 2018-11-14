module Yobo.Shared.Validation

open System

type ValidationError = {
    Name: string
    Message: string
}

type ValidationResult = {
    IsValid : bool
    Errors: ValidationError list
    TryGetMessage: string -> string option
}
with
    static member FromErrorList (errs:ValidationError list) = {
        IsValid = errs.Length = 0
        Errors = errs
        TryGetMessage = (fun x -> errs |> List.tryFind (fun e -> e.Name = x ) |> Option.map (fun x -> x.Message))
    }

    static member Empty = ValidationResult.FromErrorList []

let validateNotEmpty msg name getter args = 
    let value = args |> getter
    if String.IsNullOrWhiteSpace(value) then Some {Name = name; Message = msg } else None

let validateLongerThan l msg name getter args =
    let value : string = args |> getter
    if value.Length <= l then Some {Name = name; Message = msg } else None 

let validate (arg:'a) (fns: ('a -> ValidationError option) list) =
    fns 
    |> List.map (fun x -> arg |> x) 
    |> List.filter (Option.isSome)
    |> List.map Option.get
    |> ValidationResult.FromErrorList