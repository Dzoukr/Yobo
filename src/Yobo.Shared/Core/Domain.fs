module Yobo.Shared.Core.Domain

open System

let calculateCredits amount expiration =
    match expiration with
    | None -> amount
    | Some exp ->
        if DateTimeOffset.UtcNow > exp then 0 else amount