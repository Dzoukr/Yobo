module Yobo.Client.Pages.Login.Domain

open Yobo.Shared.Auth.Communication
open Yobo.Shared.Errors
open Yobo.Client.Forms

type Model = {
    Form : ValidatedForm<Request.Login>
}

module Model =
    let init = {
        Form = Request.Login.init |> ValidatedForm.init
    }

type Msg =
    | FormChanged of Request.Login
    | Login
    | LoggedIn of ServerResult<string>