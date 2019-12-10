module Yobo.Shared.Auth.Validation

open Yobo.Shared.Validation
open Yobo.Shared.Auth.Communication

let validateLogin (l:Request.Login) =
    [
        nameof(l.Email), validateNotEmpty l.Email
        nameof(l.Email), validateEmail l.Email
        nameof(l.Password), validateNotEmpty l.Password
    ] |> validate
    