module Yobo.Client.Pages.Login.Domain

open Yobo.Shared.Auth.Communication
open Yobo.Shared.Domain
open Yobo.Shared.Validation
open Yobo.Client.Forms

type Model = {
    IsLoading : bool
    FormSent : bool
    Form : ValidatedForm<Request.Login>
}

module Model =
    let init = {
        IsLoading = false
        FormSent = false
        Form = Request.Login.init |> ValidatedForm.init
    }

type Msg =
    | ErrorOccured of ServerError
    | FormChanged of Request.Login
    | Login
    | LoggedIn of string