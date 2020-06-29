module Yobo.Client.Pages.ResetPassword.Domain

open Yobo.Client.Forms
open Yobo.Shared.Auth.Communication
open Yobo.Shared.Errors

type Model = {
    Form : ValidatedForm<Request.ResetPassword>
}

type Msg =
    | FormChanged of Request.ResetPassword
    | Reset
    | Resetted of ServerResult<unit>