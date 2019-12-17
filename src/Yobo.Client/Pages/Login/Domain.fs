module Yobo.Client.Pages.Login.Domain

open Yobo.Shared.Auth.Communication
open Yobo.Shared.Domain
open Yobo.Shared.Validation
open Yobo.Client.Forms

type Model = {
    IsLogging : bool
    FormSent : bool
    Form : ValidatedForm<Request.Login>
}

module Model =
    let init = {
        IsLogging = false
        FormSent = false
        Form = Request.Login.init |> ValidatedForm.init
    }

type Msg =
    | FormChanged of Request.Login
    | Login
    | LoggedIn of ServerResult<string>