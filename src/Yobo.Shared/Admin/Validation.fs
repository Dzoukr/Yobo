module Yobo.Shared.Admin.Validation

open System
open Yobo.Shared.Validation
open Yobo.Shared.Text
open Yobo.Shared.Admin.Domain

let validateAddCredits (args:AddCredits) =
    [
        validateBiggerThan 0 CreditsCount (fun (x:AddCredits) -> x.Credits)
        validateIsAfter DateTime.UtcNow ExpirationDate (fun x -> x.ExpirationUtc)
        validateNotEmptyGuid Id (fun x -> x.UserId)
    ] |> validate args