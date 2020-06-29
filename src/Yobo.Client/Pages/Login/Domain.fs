module Yobo.Client.Pages.Login.Domain

open Yobo.Shared.Auth.Communication
open Yobo.Shared.Errors
open Yobo.Client.Forms

type Model = {
    Form : ValidatedForm<Request.Login>
}

type Msg =
    | FormChanged of Request.Login
    | Login
    | LoggedIn of ServerResult<string>