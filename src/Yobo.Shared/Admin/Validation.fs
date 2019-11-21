module Yobo.Shared.Admin.Validation

open System
open Yobo.Shared.Validation
open Yobo.Shared.Admin.Domain

let validateAddCredits (args:AddCredits) =
    [
        "Credits", validateBiggerThan 0 (fun (x:AddCredits) -> x.Credits)
        "Expiration", validateIsAfter DateTimeOffset.Now  (fun x -> x.Expiration)
        "UserId", validateNotEmptyGuid  (fun x -> x.UserId)
    ] |> validate args

let validateAddLesson (args:AddLesson) =
    [
        "Name", validateNotEmpty (fun (x:AddLesson) -> x.Name)
        "Description", validateNotEmpty (fun (x:AddLesson) -> x.Description)
        "End", validateIsBeforeAnother (fun x -> x.Start) (fun x -> x.End)
    ] |> validate args

let validateUpdateLesson (args:UpdateLesson) =
    [
        "Name", validateNotEmpty (fun (x:UpdateLesson) -> x.Name)
        "Description", validateNotEmpty (fun (x:UpdateLesson) -> x.Description)
        "End", validateIsBeforeAnother (fun x -> x.Start) (fun x -> x.End)
    ] |> validate args

let validateAddWorkshop (args:AddWorkshop) =
    [
        "Name", validateNotEmpty (fun (x:AddWorkshop) -> x.Name)
        "Description", validateNotEmpty (fun (x:AddWorkshop) -> x.Description)
        "End", validateIsBeforeAnother (fun x -> x.Start) (fun x -> x.End)
    ] |> validate args
