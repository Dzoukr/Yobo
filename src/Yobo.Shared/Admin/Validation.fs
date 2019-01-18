module Yobo.Shared.Admin.Validation

open System
open Yobo.Shared.Validation
open Yobo.Shared.Admin.Domain

let validateAddCredits (args:AddCredits) =
    [
        validateBiggerThan 0 "Credits" (fun (x:AddCredits) -> x.Credits)
        validateIsAfter DateTimeOffset.Now "Expiration" (fun x -> x.Expiration)
        validateNotEmptyGuid "UserId" (fun x -> x.UserId)
    ] |> validate args

let validateAddLesson (args:AddLesson) =
    [
        validateNotEmpty "Name" (fun (x:AddLesson) -> x.Name)
        validateNotEmpty "Description" (fun (x:AddLesson) -> x.Description)
        validateIsBeforeAnother "End" (fun x -> x.Start) (fun x -> x.End)
    ] |> validate args