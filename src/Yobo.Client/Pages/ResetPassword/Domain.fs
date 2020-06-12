﻿module Yobo.Client.Pages.ResetPassword.Domain

open System
open Yobo.Client.Forms
open Yobo.Shared.Auth.Communication
open Yobo.Shared.Errors

type Model = {
    Form : ValidatedForm<Request.ResetPassword>
}

module Model =
    let init key  = {
        Form = Request.ResetPassword.init |> fun x -> { x with PasswordResetKey = key } |> ValidatedForm.init
    }

type Msg =
    | FormChanged of Request.ResetPassword
    | Reset
    | Resetted of ServerResult<unit>