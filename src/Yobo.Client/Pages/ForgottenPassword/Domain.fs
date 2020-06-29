module Yobo.Client.Pages.ForgottenPassword.Domain

open Yobo.Client.Forms
open Yobo.Shared.Auth.Communication
open Yobo.Shared.Errors

type Model = {
    Form : ValidatedForm<Request.ForgottenPassword>
}

type Msg =
    | FormChanged of Request.ForgottenPassword
    | InitiateReset
    | ResetInitiated of ServerResult<unit>
