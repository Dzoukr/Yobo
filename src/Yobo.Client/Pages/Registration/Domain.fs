module Yobo.Client.Pages.Registration.Domain

open Yobo.Client.Forms
open Yobo.Shared.Auth.Communication
open Yobo.Shared.Domain
open Yobo.Shared.Validation

type Model = {
    IsLogging : bool
    FormSent : bool
    //Form : ValidatedForm
    Form : Request.Login
    FormValidationErrors : ValidationError list
}

module Model =
    let init = {
        IsLogging = false
        FormSent = false
        Form = Request.Login.init
        FormValidationErrors = []
    }

type Msg =
    | FormChanged of Request.Login
    | Login
    | LoggedIn of ServerResult<string>