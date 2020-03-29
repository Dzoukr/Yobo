module Yobo.Shared.Core.Admin.Validation

open System
open Yobo.Shared.Validation
open Communication

let validateAddCredits (l:Request.AddCredits) =
    [
        nameof(l.UserId), validateNotEmptyGuid l.UserId
        nameof(l.Credits), validateMinimumValue 1 l.Credits
        nameof(l.Expiration), validateMinimumDate DateTimeOffset.UtcNow l.Expiration
    ] |> validate
    
let validateSetExpiration (l:Request.SetExpiration) =
    [
        nameof(l.UserId), validateNotEmptyGuid l.UserId
        nameof(l.Expiration), validateMinimumDate DateTimeOffset.UtcNow l.Expiration
    ] |> validate