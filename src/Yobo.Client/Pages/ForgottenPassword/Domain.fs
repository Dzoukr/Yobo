module Yobo.Client.Pages.ForgottenPassword.Domain

open System
open Yobo.Client.Forms
open Yobo.Shared.Auth.Communication
open Yobo.Shared.Errors

type Model = {
    Form : ValidatedForm<Request.ForgottenPassword>
}

module Model =
    let init = {
        Form = Request.ForgottenPassword.init |> ValidatedForm.init
    }

type Msg =
    | FormChanged of Request.ForgottenPassword
    | InitiateReset
    | ResetInitiated of ServerResult<unit>
