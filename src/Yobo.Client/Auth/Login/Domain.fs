module Yobo.Client.Auth.Login.Domain

open Yobo.Shared.Auth.Communication
open Yobo.Shared.Communication
open Yobo.Shared.Validation

type Model = {
    IsLogging : bool
    Form : Request.Login
    FormValidationErrors : ValidationError list
}

module Model =
    let init = {
        IsLogging = false
        Form = { Email = ""; Password = "" }
        FormValidationErrors = []
    }

type Msg =
    | Navigate of string
    | FormChanged of Request.Login
    | Login
    | LoggedIn of ServerResponse<string>