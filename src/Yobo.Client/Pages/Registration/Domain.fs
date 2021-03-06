module Yobo.Client.Pages.Registration.Domain

open Yobo.Client.Forms
open Yobo.Shared.Auth.Communication
open Yobo.Shared.Errors

type Model = {
    ShowTerms : bool
    Form : ValidatedForm<Request.Register>
    ShowThankYou : bool
}

type Msg =
    | FormChanged of Request.Register
    | Register
    | Registered of ServerResult<unit>
    | ToggleTerms